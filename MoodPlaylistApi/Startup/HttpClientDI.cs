using MoodPlaylistApi.Services;
using System.Net.Http.Headers;

namespace MoodPlaylistApi.Startup
{
    public static class HttpClientDI
    {
        public static void AddSpotifyHttpClient(this WebApplicationBuilder builder)
        {
            var baseAddress = builder.Configuration["Spotify:BaseUrl"];
            var clientSecret = builder.Configuration["Spotify:ClientSecret"];
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentException("Spotify base url is required");
            if (string.IsNullOrWhiteSpace(clientSecret)) throw new ArgumentException("Spotify client secret is required");

            builder.Services.AddHttpClient<Spotify>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientSecret);
            });
        }
    }
}
 