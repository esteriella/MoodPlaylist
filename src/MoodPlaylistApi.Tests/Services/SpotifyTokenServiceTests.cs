using Microsoft.Extensions.Configuration;
using Moq;
using MoodPlaylistApi.Exceptions;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Tests.TestSupport;
using System.Net;
using System.Net.Http.Headers;

namespace MoodPlaylistApi.Tests.Services;

public sealed class SpotifyTokenServiceTests
{
    [Fact(DisplayName = "Token service sends Spotify client credentials and returns the access token")]
    public async Task GetAccessToken_ValidResponse_SendsExpectedRequestAndReturnsToken()
    {
        var handler = TokenHandler("access-token");
        var service = CreateService(handler);

        var result = await service.GetAccessToken();

        Assert.Equal("access-token", result);
        Assert.Equal(HttpMethod.Post, handler.Request?.Method);
        Assert.Equal("https://accounts.test/api/token", handler.Request?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String("client-id:client-secret"u8.ToArray())),
            handler.Request?.Headers.Authorization);
        Assert.Equal("grant_type=client_credentials", handler.RequestContent);
    }

    [Fact(DisplayName = "Token service reuses a valid cached access token")]
    public async Task GetAccessToken_TokenAlreadyLoaded_ReusesTokenWithoutAnotherRequest()
    {
        var handler = TokenHandler("cached-token");
        var service = CreateService(handler);

        var first = await service.GetAccessToken();
        var second = await service.GetAccessToken();

        Assert.Equal(first, second);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact(DisplayName = "Concurrent token requests share one Spotify authentication call")]
    public async Task GetAccessToken_ConcurrentRequests_RequestsTokenOnce()
    {
        var handler = TokenHandler("shared-token");
        var service = CreateService(handler);

        var results = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => service.GetAccessToken()));

        Assert.All(results, token => Assert.Equal("shared-token", token));
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact(DisplayName = "Token service maps an unsuccessful Spotify response to SpotifyApiException")]
    public async Task GetAccessToken_UnsuccessfulResponse_ThrowsSpotifyApiException()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse("{}", HttpStatusCode.Unauthorized));
        var service = CreateService(handler);

        var exception = await Assert.ThrowsAsync<SpotifyApiException>(() => service.GetAccessToken());

        Assert.Contains("401", exception.Message);
        Assert.Contains("Unauthorized", exception.Message);
    }

    [Fact(DisplayName = "Token service rejects a successful response without an access token")]
    public async Task GetAccessToken_ResponseHasNoAccessToken_ThrowsSpotifyApiException()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse("{\"expires_in\":3600}"));
        var service = CreateService(handler);

        await Assert.ThrowsAsync<SpotifyApiException>(() => service.GetAccessToken());
    }

    private static SpotifyTokenService CreateService(RecordingHttpMessageHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://accounts.test/") };
        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(x => x.CreateClient("SpotifyAccounts")).Returns(client);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Spotify:ClientId"] = "client-id",
                ["Spotify:ClientSecret"] = "client-secret"
            })
            .Build();
        return new SpotifyTokenService(factory.Object, configuration);
    }

    private static RecordingHttpMessageHandler TokenHandler(string token) => new(_ =>
        RecordingHttpMessageHandler.JsonResponse(
            $"{{\"access_token\":\"{token}\",\"expires_in\":3600}}"));
}
