using MoodPlaylistApi.Exceptions;
using System.Text.Json;

namespace MoodPlaylistApi.Services
{
    public interface ISpotifyService
    {
        Task<string> GetTracksForMood(string moodName);
        Task<string> GetTrackById(string trackId);
    }

    public sealed class SpotifyService(HttpClient httpClient) : ISpotifyService
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<string> GetTracksForMood(string moodName)
        {

            var response = await _httpClient.GetAsync(
                $"v1/search?q={moodName}&type=track&limit=10" 
            );
            if (!response.IsSuccessStatusCode)
            {
                throw new SpotifyApiException(
                     $"Failed.\n Content\n:{JsonSerializer.Serialize(response.Content)}");
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetTrackById(string trackId)
        {

            var response = await _httpClient.GetAsync($"v1/tracks/{trackId}");
            if (!response.IsSuccessStatusCode)
            {
                if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new TrackNotFoundException(trackId);

                throw new SpotifyApiException(
                     $"Failed.\n Content\n:{JsonSerializer.Serialize(response.Content)}");
            }

            return await response.Content.ReadAsStringAsync();
        }

    }

}
