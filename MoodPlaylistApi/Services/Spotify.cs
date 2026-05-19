using System.Net.Http.Headers;

namespace MoodPlaylistApi.Services
{
    public class Spotify(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<string> GetTracksForMood(string moodName, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(
                $"https://api.spotify.com/v1/search?q={moodName}&type=track&limit=10"
            );
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> GetTrackById(string trackId, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/tracks/{trackId}");
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadAsStringAsync();
        }

    }

}
