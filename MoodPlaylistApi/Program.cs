using MoodPlaylistApi.Helpers;
using MoodPlaylistApi.Middleware;
using MoodPlaylistApi.Startup;
// If you still get errors, ensure the Swashbuckle.AspNetCore NuGet package is referenced in the project.

var builder = WebApplication.CreateBuilder(args);
Logging.Main(builder);

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Starting application...");

Database.ConfigureDatabase(builder);

AuthDI.AddJwt(builder);

HttpClientDI.AddSpotifyHttpClient(builder);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Swagger (optional, but useful for testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
JwtSettingsHelper.JwtConfigure(app.Services.GetRequiredService<IConfiguration>());
app.UseMiddleware<ExceptionMiddleware>();

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
