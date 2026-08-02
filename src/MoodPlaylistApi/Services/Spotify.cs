using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Exceptions;
using System.Globalization;
using System.Text.Json;

namespace MoodPlaylistApi.Services
{
    public interface ISpotifyService
    {
        Task<string> GetTracksForMood(string moodName);
        Task<string> GetTrackById(string trackId);

        // Define method for getting recommandations based on mood's seed genres and audio features -> deserialize the response to a list of tracks and return it to the controller
        Task<List<Track>> GetTracksByMoodRecommendations(List<string> seedGenres, Dictionary<string, Dictionary<string, double>> audioFeatures);
        Task<List<Track>> GetRecommendations(
            IReadOnlyCollection<string> seedGenres,
            IReadOnlyCollection<string> seedTracks,
            IReadOnlyDictionary<string, Dictionary<string, double>> audioFeatures,
            int limit = 20,
            string? market = null);
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
            => await GetRecommendations(seedGenres, [], audioFeatures);

        public async Task<List<Track>> GetRecommendations(
            IReadOnlyCollection<string> seedGenres,
            IReadOnlyCollection<string> seedTracks,
            IReadOnlyDictionary<string, Dictionary<string, double>> audioFeatures,
            int limit = 20,
            string? market = null)
        {
            List<string> queryParams = [];

            if (seedGenres.Count > 0)
                queryParams.Add($"seed_genres={EncodeList(seedGenres)}");
            if (seedTracks.Count > 0)
                queryParams.Add($"seed_tracks={EncodeList(seedTracks)}");

            queryParams.Add($"limit={limit.ToString(CultureInfo.InvariantCulture)}");
            if (!string.IsNullOrWhiteSpace(market))
                queryParams.Add($"market={Uri.EscapeDataString(market.ToUpperInvariant())}");

            foreach (var feature in audioFeatures.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                foreach (var constraint in feature.Value.OrderBy(x => x.Key, StringComparer.Ordinal))
                {
                    queryParams.Add(
                        $"{constraint.Key}_{feature.Key}={constraint.Value.ToString(CultureInfo.InvariantCulture)}");
                }
            }

            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"v1/recommendations?{queryString}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new SpotifyApiException(
                    $"Spotify recommendations request failed with status {(int)response.StatusCode} ({response.StatusCode}). {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var recommendationsResponse = JsonSerializer.Deserialize<SpotifyRecommendationsResponse>(content);
            return recommendationsResponse?.Tracks ?? [];
        }

        private static string EncodeList(IEnumerable<string> values) =>
            string.Join(",", values.Select(value => Uri.EscapeDataString(value)));
    }
}
