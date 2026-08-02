using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MoodPlaylistApi.Data;

namespace MoodPlaylistApi.Tests.Integration;

public sealed class MoodPlaylistApiFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlFixture _database;
    private readonly Dictionary<string, string?> _previousEnvironment = [];

    public MoodPlaylistApiFactory(PostgreSqlFixture database)
    {
        _database = database;
        SetEnvironment("ASPNETCORE_ENVIRONMENT", "Testing");
        SetEnvironment("ConnectionStrings__DefaultConnection", database.ConnectionString);
        SetEnvironment("Jwt__Key", "testcontainers-signing-key-with-at-least-32-characters");
        SetEnvironment("Jwt__Issuer", "https://moodplaylist.test");
        SetEnvironment("Jwt__Audience", "moodplaylist-tests");
        SetEnvironment("Jwt__MaxAge", "10");
        SetEnvironment("Jwt__MaxRefreshAge", "24");
        SetEnvironment("HashHelper__SecretKey", "testcontainers-hash-secret");
        SetEnvironment("Spotify__BaseUrl", "https://spotify.test/");
        SetEnvironment("Spotify__AccountsBaseUrl", "https://accounts.test/");
        SetEnvironment("Spotify__ClientId", "test-client-id");
        SetEnvironment("Spotify__ClientSecret", "test-client-secret");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_database.ConnectionString));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        foreach (var setting in _previousEnvironment)
            Environment.SetEnvironmentVariable(setting.Key, setting.Value);
    }

    private void SetEnvironment(string key, string value)
    {
        _previousEnvironment[key] = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
    }
}
