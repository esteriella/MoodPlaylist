using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Startup;
using MoodPlaylistApi.Tests.TestSupport;
using System.Net.Http.Headers;

namespace MoodPlaylistApi.Tests.Startup;

public sealed class HttpClientDITests
{
    [Fact(DisplayName = "Spotify registration obtains and attaches an access token")]
    public async Task AddSpotifyHttpClient_ValidConfiguration_ObtainsAndAttachesAccessToken()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Spotify:BaseUrl"] = "https://spotify.test/",
            ["Spotify:AccountsBaseUrl"] = "https://accounts.test/",
            ["Spotify:ClientId"] = "client-id",
            ["Spotify:ClientSecret"] = "client-secret"
        });
        var apiHandler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse("{\"id\":\"track-1\"}"));
        var tokenHandler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse(
                "{\"access_token\":\"access-token\",\"token_type\":\"Bearer\",\"expires_in\":3600}"));
        builder.AddSpotifyHttpClient();
        builder.Services.AddHttpClient("SpotifyAccounts")
            .ConfigurePrimaryHttpMessageHandler(() => tokenHandler);
        builder.Services
            .AddHttpClient<ISpotifyService, SpotifyService>()
            .ConfigurePrimaryHttpMessageHandler(() => apiHandler);
        await using var provider = builder.Services.BuildServiceProvider();
        var service = provider.GetRequiredService<ISpotifyService>();

        await service.GetTrackById("track-1");

        Assert.Equal("https://accounts.test/api/token", tokenHandler.Request?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String("client-id:client-secret"u8.ToArray())),
            tokenHandler.Request?.Headers.Authorization);
        Assert.Equal("https://spotify.test/v1/tracks/track-1", apiHandler.Request?.RequestUri?.AbsoluteUri);
        Assert.Equal(new AuthenticationHeaderValue("Bearer", "access-token"), apiHandler.Request?.Headers.Authorization);
    }

    [Fact(DisplayName = "Spotify registration rejects a missing base URL")]
    public void AddSpotifyHttpClient_MissingBaseUrl_ThrowsArgumentException()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Spotify:ClientSecret"] = "test-token"
        });

        var exception = Assert.Throws<ArgumentException>(() => builder.AddSpotifyHttpClient());

        Assert.Equal("Spotify base url is required", exception.Message);
    }

    [Fact(DisplayName = "Spotify registration rejects a missing client ID")]
    public void AddSpotifyHttpClient_MissingClientId_ThrowsArgumentException()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Spotify:BaseUrl"] = "https://spotify.test/",
            ["Spotify:ClientSecret"] = "client-secret"
        });

        var exception = Assert.Throws<ArgumentException>(() => builder.AddSpotifyHttpClient());

        Assert.Equal("Spotify client id is required", exception.Message);
    }

    [Fact(DisplayName = "Spotify registration rejects a missing client secret")]
    public void AddSpotifyHttpClient_MissingClientSecret_ThrowsArgumentException()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Spotify:BaseUrl"] = "https://spotify.test/",
            ["Spotify:ClientId"] = "client-id"
        });

        var exception = Assert.Throws<ArgumentException>(() => builder.AddSpotifyHttpClient());

        Assert.Equal("Spotify client secret is required", exception.Message);
    }

    private static WebApplicationBuilder CreateBuilder(Dictionary<string, string?> configuration)
    {
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
        builder.Configuration.AddInMemoryCollection(configuration);
        return builder;
    }
}
