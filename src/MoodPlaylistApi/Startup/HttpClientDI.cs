using MoodPlaylistApi.Services;

namespace MoodPlaylistApi.Startup
{
    public static class HttpClientDI
    {
        public static void AddSpotifyHttpClient(this WebApplicationBuilder builder)
        {
            var baseAddress = builder.Configuration["Spotify:BaseUrl"];
            var accountsBaseAddress = builder.Configuration["Spotify:AccountsBaseUrl"]
                ?? "https://accounts.spotify.com/";
            var clientId = builder.Configuration["Spotify:ClientId"];
            var clientSecret = builder.Configuration["Spotify:ClientSecret"];
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentException("Spotify base url is required");
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException("Spotify client id is required");
            if (string.IsNullOrWhiteSpace(clientSecret)) throw new ArgumentException("Spotify client secret is required");

            builder.Services.AddHttpClient("SpotifyAccounts", client =>
            {
                client.BaseAddress = new Uri(accountsBaseAddress);
                client.Timeout = TimeSpan.FromSeconds(15);
            });
            builder.Services.AddSingleton<ISpotifyTokenService, SpotifyTokenService>();
            builder.Services.AddTransient<SpotifyAuthenticationHandler>();

            builder.Services.AddHttpClient<ISpotifyService, SpotifyService>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = TimeSpan.FromSeconds(15);
            }).AddHttpMessageHandler<SpotifyAuthenticationHandler>();
        }
    }
}
