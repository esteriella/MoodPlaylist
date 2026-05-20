using MoodPlaylistApi.Data;
using MoodPlaylistApi.Helpers;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Middlewares;
using MoodPlaylistApi.Startup;
using MoodPlaylistApi.Utilities;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
// If you still get errors, ensure the Swashbuckle.AspNetCore NuGet package is referenced in the project.

var builder = WebApplication.CreateBuilder(args);
Logging.Main(builder);

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Starting application...");

Database.ConfigureDatabase(builder);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

AuthDI.AddJwt(builder);

HttpClientDI.AddSpotifyHttpClient(builder);

builder.Services.AddControllers();
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
            builder.WithOrigins("https://example.com")
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
        });
});
// Swagger (optional, but useful for testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
JwtSettingsHelper.JwtConfigure(app.Services.GetRequiredService<IConfiguration>());
HashHelperSettings.Configure(app.Services.GetRequiredService<IConfiguration>());
app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();
app.UseCors();
// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<AuthMiddleware>();
app.MapControllers();

logger.LogInformation("Running application...");
app.Run();
