using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MoodPlaylistApi.Data;

namespace MoodPlaylistApi.Tests.Integration;

public sealed class MoodPlaylistApiFactory(PostgreSqlFixture database)
    : WebApplicationFactory<Program>
{
    private Dictionary<string, string?> TestConfiguration => new()
    {
        ["ConnectionStrings:DefaultConnection"] = database.ConnectionString,
        ["Jwt:Key"] = "testcontainers-signing-key-with-at-least-32-characters",
        ["Jwt:Issuer"] = "https://moodplaylist.test",
        ["Jwt:Audience"] = "moodplaylist-tests",
        ["Jwt:MaxAge"] = "10",
        ["Jwt:MaxRefreshAge"] = "24",
        ["HashHelper:SecretKey"] = "testcontainers-hash-secret",
        ["Spotify:BaseUrl"] = "https://spotify.test/",
        ["Spotify:AccountsBaseUrl"] = "https://accounts.test/",
        ["Spotify:ClientId"] = "test-client-id",
        ["Spotify:ClientSecret"] = "test-client-secret"
    };

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(TestConfiguration));
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(database.ConnectionString));
        });
    }
}
