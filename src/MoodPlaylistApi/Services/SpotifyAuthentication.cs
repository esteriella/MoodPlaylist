using MoodPlaylistApi.Exceptions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

namespace MoodPlaylistApi.Services
{
    public interface ISpotifyTokenService
    {
        Task<string> GetAccessToken(CancellationToken cancellationToken = default);
    }

    public sealed class SpotifyTokenService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration) : ISpotifyTokenService
    {
        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private string? _accessToken;
        private DateTimeOffset _expiresAt;

        public async Task<string> GetAccessToken(CancellationToken cancellationToken = default)
        {
            if (TokenIsValid()) return _accessToken!;

            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (TokenIsValid()) return _accessToken!;

                var clientId = configuration["Spotify:ClientId"]!;
                var clientSecret = configuration["Spotify:ClientSecret"]!;
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/token")
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "client_credentials"
                    })
                };
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

                var response = await httpClientFactory.CreateClient("SpotifyAccounts")
                    .SendAsync(request, cancellationToken);
                var token = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>(
                    cancellationToken: cancellationToken);

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(token?.AccessToken))
                    throw new SpotifyApiException(
                        $"Spotify authentication failed with status {(int)response.StatusCode} ({response.StatusCode}).");

                _accessToken = token.AccessToken;
                _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(30, token.ExpiresIn - 60));
                return _accessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private bool TokenIsValid() =>
            !string.IsNullOrWhiteSpace(_accessToken) && DateTimeOffset.UtcNow < _expiresAt;

        private sealed record SpotifyTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; init; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; init; }
        }
    }

    public sealed class SpotifyAuthenticationHandler(ISpotifyTokenService tokenService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                await tokenService.GetAccessToken(cancellationToken));
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
