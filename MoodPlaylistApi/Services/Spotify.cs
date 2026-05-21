using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Exceptions;
using System.Text.Json;

namespace MoodPlaylistApi.Services
{
    public interface ISpotifyService
    {
        Task<string> GetTracksForMood(string moodName);
        Task<string> GetTrackById(string trackId);

        // Define method for getting recommandations based on mood's seed genres and audio features -> deserialize the response to a list of tracks and return it to the controller
        Task<List<Track>> GetTracksByMoodRecommendations(List<string> seedGenres, Dictionary<string, Dictionary<string, double>> audioFeatures);
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
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new TrackNotFoundException(trackId);

                throw new SpotifyApiException(
                     $"Failed.\n Content\n:{JsonSerializer.Serialize(response.Content)}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<Track>> GetTracksByMoodRecommendations(List<string> seedGenres, Dictionary<string, Dictionary<string, double>> audioFeatures)
        {
            List<string> queryParams =
            [
                // because seed genres is required, we automatically add it and check in upper layer to see if null 
                $"seed_genres={string.Join(",", seedGenres)}"
            ];

            // this check seems irrelevant because, it seems audio features will always be provide, not so sure though
            if (audioFeatures is not null)
            {
                // this nested foreaach is too expensive, we can optimize it by using LINQ to flatten the dictionary and create the query parameters in one go
                foreach (var feature in audioFeatures)
                {
                    foreach (var constraint in feature.Value)
                    {
                        queryParams.Add($"{feature.Key}_{constraint.Key}={constraint.Value}");
                    }
                }
            }
            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"v1/recommendations?{queryString}");
            if (!response.IsSuccessStatusCode)
                throw new SpotifyApiException(
                     $"Failed.\n Content\n:{JsonSerializer.Serialize(response.Content)}");

            var content = await response.Content.ReadAsStringAsync();
            var recommendationsResponse = JsonSerializer.Deserialize<SpotifyRecommendationsResponse>(content);
            return recommendationsResponse?.Tracks ?? [];
        }
    }
}
