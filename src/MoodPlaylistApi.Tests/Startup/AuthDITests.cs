using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MoodPlaylistApi.Startup;

namespace MoodPlaylistApi.Tests.Startup;

public sealed class AuthDITests
{
    [Fact(DisplayName = "JWT registration configures issuer audience signing key and development metadata")]
    public void AddJwt_ValidDevelopmentConfiguration_ConfiguresBearerOptions()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "https://issuer.test",
            ["Jwt:Audience"] = "audience.test",
            ["Jwt:Key"] = "this-is-a-test-signing-key-with-at-least-32-characters"
        });
        builder.AddJwt();
        using var provider = builder.Services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal("https://issuer.test", options.TokenValidationParameters.ValidIssuer);
        Assert.Equal("audience.test", options.TokenValidationParameters.ValidAudience);
        Assert.True(options.TokenValidationParameters.ValidateLifetime);
        Assert.False(options.RequireHttpsMetadata);
        Assert.NotNull(options.TokenValidationParameters.IssuerSigningKey);
    }

    [Fact(DisplayName = "JWT registration requires HTTPS metadata outside development")]
    public void AddJwt_ValidProductionConfiguration_RequiresHttpsMetadata()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "https://issuer.test",
            ["Jwt:Audience"] = "audience.test",
            ["Jwt:Key"] = "this-is-a-test-signing-key-with-at-least-32-characters"
        }, "Production");
        builder.AddJwt();
        using var provider = builder.Services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.True(options.RequireHttpsMetadata);
    }

    public static TheoryData<string, string> MissingSettings => new()
    {
        { "Jwt:Issuer", "Auth authority not configured." },
        { "Jwt:Audience", "Auth audience not configured." },
        { "Jwt:Key", "Auth key not configured." }
    };

    [Theory(DisplayName = "JWT registration rejects missing required configuration")]
    [MemberData(nameof(MissingSettings))]
    public void AddJwt_RequiredSettingMissing_ThrowsInvalidOperationException(
        string missingSetting,
        string expectedMessage)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "issuer.test",
            ["Jwt:Audience"] = "audience.test",
            ["Jwt:Key"] = "this-is-a-test-signing-key-with-at-least-32-characters"
        };
        settings.Remove(missingSetting);
        var builder = CreateBuilder(settings);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.AddJwt());

        Assert.Equal(expectedMessage, exception.Message);
    }

    private static WebApplicationBuilder CreateBuilder(
        Dictionary<string, string?> configuration,
        string environmentName = "Development")
    {
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            EnvironmentName = environmentName
        });
        builder.Configuration.AddInMemoryCollection(configuration);
        return builder;
    }
}
