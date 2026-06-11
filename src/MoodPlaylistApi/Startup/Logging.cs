using Serilog;
using Serilog.Events;
using SerilogTracing;
using System.Globalization;

namespace MoodPlaylistApi.Startup
{
    public static class Logging
    {
        public static void Main(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", "Mood Playlist API")
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .WriteTo.Console()
                 // Writting to file seems uneccesary as app will be containerized
                 .WriteTo.File(
                     "logs/api-.log",
                     rollingInterval: RollingInterval.Day,
                     outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                     formatProvider: CultureInfo.InvariantCulture
                 )
                .CreateBootstrapLogger();

            builder.Host.UseSerilog(
                (ctx, services, config) =>
                {
                    config
                        .ReadFrom.Configuration(ctx.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .WriteTo.Console();
                }
            );

            // Activity tracing MUST be after Serilog is attached
            if (builder.Environment.IsDevelopment())
            {
                new ActivityListenerConfiguration().TraceToSharedLogger();
            }
        }
    }
}
