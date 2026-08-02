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
    [Fact(DisplayName = "Spotify registration configures the base URL and bearer token")]
    public async Task AddSpotifyHttpClient_ValidConfiguration_ConfiguresTypedClient()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Spotify:BaseUrl"] = "https://spotify.test/",
            ["Spotify:ClientSecret"] = "test-token"
        });
        var handler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse("{\"id\":\"track-1\"}"));
        builder.AddSpotifyHttpClient();
        builder.Services
            .AddHttpClient<ISpotifyService, SpotifyService>()
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        await using var provider = builder.Services.BuildServiceProvider();
        var service = provider.GetRequiredService<ISpotifyService>();

        await service.GetTrackById("track-1");

        Assert.Equal("https://spotify.test/v1/tracks/track-1", handler.Request?.RequestUri?.AbsoluteUri);
        Assert.Equal(new AuthenticationHeaderValue("Bearer", "test-token"), handler.Request?.Headers.Authorization);
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

    [Fact(DisplayName = "Spotify registration rejects a missing client secret")]
    public void AddSpotifyHttpClient_MissingClientSecret_ThrowsArgumentException()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Spotify:BaseUrl"] = "https://spotify.test/"
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
