using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Startup;
using Swashbuckle.AspNetCore.SwaggerGen; // Add this using for AddSwaggerGen extension
// If you still get errors, ensure the Swashbuckle.AspNetCore NuGet package is referenced in the project.

var builder = WebApplication.CreateBuilder(args);
Logging.Main(builder);

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Starting application...");

Database.ConfigureDatabase(builder);

builder.Services.AddHttpClient<Spotify>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Swagger (optional, but useful for testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<MoodPlaylistApi.Middleware.ErrorHandling>();

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

app.MapControllers();

logger.LogInformation("Running application...");
app.Run();
