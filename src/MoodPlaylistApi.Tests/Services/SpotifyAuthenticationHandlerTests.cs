using Moq;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Tests.TestSupport;
using System.Net.Http.Headers;

namespace MoodPlaylistApi.Tests.Services;

public sealed class SpotifyAuthenticationHandlerTests
{
    [Fact(DisplayName = "Authentication handler attaches the current Spotify bearer token")]
    public async Task SendAsync_RequestSent_AttachesBearerTokenAndReturnsResponse()
    {
        var tokenService = new Mock<ISpotifyTokenService>(MockBehavior.Strict);
        tokenService.Setup(x => x.GetAccessToken(It.IsAny<CancellationToken>()))
            .ReturnsAsync("access-token");
        var terminalHandler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse("{}"));
        var authenticationHandler = new SpotifyAuthenticationHandler(tokenService.Object)
        {
            InnerHandler = terminalHandler
        };
        using var client = new HttpClient(authenticationHandler)
        {
            BaseAddress = new Uri("https://spotify.test/")
        };
        using var request = new HttpRequestMessage(HttpMethod.Get, "v1/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "stale-token");

        var response = await client.SendAsync(request);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(
            new AuthenticationHeaderValue("Bearer", "access-token"),
            terminalHandler.Request?.Headers.Authorization);
        tokenService.Verify(x => x.GetAccessToken(It.IsAny<CancellationToken>()), Times.Once);
    }
}
