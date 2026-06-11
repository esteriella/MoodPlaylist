
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MoodPlaylistApi.Data;
using MoodPlaylistApi.Helpers;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Middlewares;
using MoodPlaylistApi.Startup;
using MoodPlaylistApi.Utilities;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
Logging.Main(builder);

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Starting application...");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
Database.ConfigureDatabase(builder, connectionString);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

AuthDI.AddJwt(builder);

HttpClientDI.AddSpotifyHttpClient(builder);

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        Guid id = Guid.CreateVersion7();
        string exId = $"ERR-{DateTime.UtcNow:ddmmyy}-{id}";
        // API-specific model state error handling
        var errorMessages = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .Distinct();

        var errorMessage = string.Join("; ", errorMessages);
        logger.LogError("Model validation failed. Error ID: {ExceptionId}. Errors: {Errors}", exId, errorMessages);
        return new BadRequestObjectResult(ApiResponse<string>.Error(
            HttpStatusCode.UnprocessableEntity,
            $"Unable to process your request with errors:\n{errorMessage}.\n Contact support with error id: {exId}"
        ));
    };
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Configure global rate limiting for the application
builder.Services.AddRateLimiter(options =>
{
    // Define behavior when a request is rejected due to rate limiting
    options.OnRejected = async (context, _) =>
    {
        // If metadata contains a RetryAfter value, add it to the response headers
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

        // Build a standardized error response using ApiResponse wrapper
        var errorResponse = ApiResponse<string>.Error(
            HttpStatusCode.TooManyRequests,
            "Too many requests. Please try again later."
        );

        // Set HTTP status code and content type for the response
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        // Write the serialized error response to the body
        await context.HttpContext.Response
            .WriteAsync(JsonSerializer.Serialize(errorResponse), cancellationToken: _);
    };

    // Configure a global rate limiter using partitioned logic
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Check if the user is authenticated
        var isAuthenticated = httpContext.User.Identity != null &&
                              httpContext.User.Identity.IsAuthenticated;

        // Partition key: use user ID if authenticated, otherwise use client IP
        var partitionKey = isAuthenticated
            ? httpContext.User.FindFirst(CustomClaimTypes.UserId)?.Value ?? "anonymous"
            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

        // Define fixed window rate limiting options
        var rateLimitOptions = new FixedWindowRateLimiterOptions
        {
            // Authenticated users get a higher limit (20 requests/minute)
            // Anonymous users get a lower limit (10 requests/minute)
            PermitLimit = isAuthenticated ? 20 : 10,

            // Time window for counting requests
            Window = TimeSpan.FromMinutes(1),

            // Requests are queued in oldest-first order if limit is exceeded
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,

            // Maximum number of queued requests allowed
            QueueLimit = 2
        };

        // Return a fixed window limiter for the given partition key
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => rateLimitOptions);
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .AllowCredentials();
        });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: connectionString,
        name: "Mood Playlist Database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "postgres"]
    )
    .AddUrlGroup(
        new Uri("https://api.spotify.com"),
        name: "Spotify API",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["external", "api"]
    );


var app = builder.Build();

JwtSettingsHelper.JwtConfigure(app.Services.GetRequiredService<IConfiguration>());
HashHelperSettings.Configure(app.Services.GetRequiredService<IConfiguration>());

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = report.Status == HealthStatus.Healthy ? 200 :
                              report.Status == HealthStatus.Degraded ? 503 : 500;

        var descriptions = new Dictionary<string, string>
        {
            ["Mood Playlist Database"] = "MoodPlaylist database is operational and accepting connections.",
            ["Spotify API"] = "Spotify recommendation API is reachable and responding to requests."
        };

        var uptimeSpan = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime());
        var appInfo = new
        {
            name = "MoodPlaylist Web Api",
            version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
            environment = app.Environment.EnvironmentName ?? Environments.Development,
            uptimeHours = $"{(int)uptimeSpan.TotalDays}d {(int)uptimeSpan.TotalHours}h {uptimeSpan.Minutes}m {uptimeSpan.Seconds}s"
        };

        var json = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            app = appInfo,
            checks = report.Entries.Select(e => new
            {
                key = e.Key,
                status = e.Value.Status.ToString(),
                duration = $"{e.Value.Duration.TotalSeconds}s {e.Value.Duration.TotalMilliseconds}ms",
                description = e.Value.Description ?? descriptions.GetValueOrDefault(e.Key)
            })
        });

        await context.Response.WriteAsync(json);
    }
});

app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();
app.UseCors();

// Enable Swagger UI in development
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHsts();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuthMiddleware>();
app.MapControllers();

logger.LogInformation("Running application...");
app.Run();
