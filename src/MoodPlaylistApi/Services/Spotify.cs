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
        private const int SpotifySearchPageSize = 10;
        private readonly HttpClient _httpClient = httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task<string> GetTracksForMood(string moodName)
        {
            var response = await _httpClient.GetAsync(
                $"v1/search?q={Uri.EscapeDataString(moodName)}&type=track&limit=10");
            await EnsureSpotifySuccess(response, "track search");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetTrackById(string trackId)
        {
            var response = await _httpClient.GetAsync($"v1/tracks/{Uri.EscapeDataString(trackId)}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new TrackNotFoundException(trackId);

            await EnsureSpotifySuccess(response, "track lookup");
            return await response.Content.ReadAsStringAsync();
        }

        public Task<List<Track>> GetTracksByMoodRecommendations(
            List<string> seedGenres,
            Dictionary<string, Dictionary<string, double>> audioFeatures) =>
            GetRecommendations(seedGenres, [], audioFeatures);

        public async Task<List<Track>> GetRecommendations(
            IReadOnlyCollection<string> seedGenres,
            IReadOnlyCollection<string> seedTracks,
            IReadOnlyDictionary<string, Dictionary<string, double>> audioFeatures,
            int limit = 20,
            string? market = null)
        {
            // Spotify removed Recommendations and Audio Features access for new apps.
            // Mood genres and seed-track artists now drive supported catalog searches.
            _ = audioFeatures;

            var excludedTrackIds = seedTracks.ToHashSet(StringComparer.Ordinal);
            var searchQueries = seedGenres
                .Where(genre => !string.IsNullOrWhiteSpace(genre))
                .Select(genre => $"genre:\"{EscapeFilterValue(genre)}\"")
                .ToList();

            foreach (var trackId in excludedTrackIds)
            {
                var seedTrack = await GetTrack(trackId);
                searchQueries.AddRange(seedTrack.Artists
                    .Where(artist => !string.IsNullOrWhiteSpace(artist.Name))
                    .Select(artist => $"artist:\"{EscapeFilterValue(artist.Name)}\""));
            }

            searchQueries = searchQueries.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (searchQueries.Count == 0)
                return [];

            var results = new List<Track>(limit);
            var resultIds = new HashSet<string>(StringComparer.Ordinal);
            var offsets = searchQueries.ToDictionary(query => query, _ => 0, StringComparer.OrdinalIgnoreCase);
            var activeQueries = new HashSet<string>(searchQueries, StringComparer.OrdinalIgnoreCase);

            while (results.Count < limit && activeQueries.Count > 0)
            {
                var queriesThisRound = searchQueries.Where(activeQueries.Contains).ToList();
                var pages = await Task.WhenAll(queriesThisRound.Select(async query =>
                    (Query: query, Page: await SearchTracks(query, offsets[query], market))));

                foreach (var resultPage in pages)
                {
                    var query = resultPage.Query;
                    var page = resultPage.Page;
                    if (page.Items.Count == 0 || string.IsNullOrWhiteSpace(page.Next))
                        activeQueries.Remove(query);
                    else
                        offsets[query] += SpotifySearchPageSize;

                    foreach (var track in page.Items)
                    {
                        if (!excludedTrackIds.Contains(track.Id) && resultIds.Add(track.Id))
                            results.Add(track);
                        if (results.Count == limit)
                            break;
                    }

                    if (results.Count == limit)
                        break;
                }
            }

            return results;
        }

        private async Task<Track> GetTrack(string trackId)
        {
            var response = await _httpClient.GetAsync($"v1/tracks/{Uri.EscapeDataString(trackId)}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new TrackNotFoundException(trackId);

            await EnsureSpotifySuccess(response, "recommendation seed lookup");
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Track>(content, JsonOptions)
                ?? throw new SpotifyApiException("Spotify returned an invalid track response.");
        }

        private async Task<SpotifyTrackPage> SearchTracks(string query, int offset, string? market)
        {
            var queryParams = new List<string>
            {
                $"q={Uri.EscapeDataString(query)}",
                "type=track",
                $"limit={SpotifySearchPageSize.ToString(CultureInfo.InvariantCulture)}",
                $"offset={offset.ToString(CultureInfo.InvariantCulture)}"
            };
            if (!string.IsNullOrWhiteSpace(market))
                queryParams.Add($"market={Uri.EscapeDataString(market.ToUpperInvariant())}");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync($"v1/search?{string.Join("&", queryParams)}");
            }
            catch (TaskCanceledException exception)
            {
                throw new SpotifyApiException("Spotify recommendation search timed out.", exception);
            }
            await EnsureSpotifySuccess(response, "recommendation search");
            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<SpotifySearchResponse>(content, JsonOptions);
            return searchResponse?.Tracks ?? new SpotifyTrackPage();
        }

        private static async Task EnsureSpotifySuccess(HttpResponseMessage response, string operation)
        {
            if (response.IsSuccessStatusCode)
                return;

            var error = await response.Content.ReadAsStringAsync();
            throw new SpotifyApiException(
                $"Spotify {operation} failed with status {(int)response.StatusCode} ({response.StatusCode}). {error}");
        }

        private static string EscapeFilterValue(string value) =>
            value.Trim().Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
