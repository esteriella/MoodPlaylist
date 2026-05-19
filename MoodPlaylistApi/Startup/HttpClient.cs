using MoodPlaylistApi.Services;
using System.Net.Http.Headers;

namespace MoodPlaylistApi.Startup
{
    public static class HttpClient
    {
        // Call from Program.cs (or wherever 'builder' is declared):
        // builder.AddSpotifyHttpClient(configuration["Spotify:BaseAddress"], configuration["Spotify:BearerToken"]);
        public static void AddSpotifyHttpClient(this WebApplicationBuilder builder, string baseAddress, string bearerToken)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Configuration.GetSection("Spotify");
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentException("Base address is required", nameof(baseAddress));
            if (string.IsNullOrWhiteSpace(bearerToken)) throw new ArgumentException("Bearer token is required", nameof(bearerToken));

            builder.Services.AddHttpClient<Spotify>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            });
        }
    }
}
 