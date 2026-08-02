using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoodPlaylistApi.Data;
using MoodPlaylistApi.Helpers;
using Testcontainers.PostgreSql;

namespace MoodPlaylistApi.Tests.Integration;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("moodplaylist_tests")
        .WithUsername("moodplaylist")
        .WithPassword("moodplaylist_tests")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
        ConfigureStaticSettings();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new AppDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Playlists\", \"Users\" CASCADE;");
    }

    private static void ConfigureStaticSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "testcontainers-signing-key-with-at-least-32-characters",
                ["Jwt:Issuer"] = "moodplaylist-tests",
                ["Jwt:Audience"] = "moodplaylist-tests",
                ["Jwt:MaxAge"] = "10",
                ["Jwt:MaxRefreshAge"] = "24",
                ["HashHelper:SecretKey"] = "testcontainers-hash-secret"
            })
            .Build();
        JwtSettingsHelper.JwtConfigure(configuration);
        HashHelperSettings.Configure(configuration);
    }
}

[CollectionDefinition("PostgreSQL", DisableParallelization = true)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>;
