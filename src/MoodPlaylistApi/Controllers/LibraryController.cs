using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Exceptions;
using MoodPlaylistApi.Extensions;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Utilities;
using System.Net;

namespace MoodPlaylistApi.Controllers
{
    [Authorize]
    [Route("library")]
    public class LibraryController(
        IUnitOfWork uow,
        ISpotifyService spotifyService,
        ICacheService cacheService) : BaseController
    {
        [AllowAnonymous]
        [HttpGet("available-moods")]
        public async Task<IActionResult> GetAvailableMoods()
        {
            var response = await uow.LibraryRepository.GetAvailableMoods();
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("available-moods/{id}/tracks")]
        public async Task<IActionResult> GetAvailableMoodTracks([FromRoute] Guid id)
        {
            Mood mood = await uow.LibraryRepository.GetByIdAsync(id) ?? throw new MoodNotFoundException(id);
            List<string> seedGenres = mood.GetSeedGenres();
            if (seedGenres is { Count: 0})
                throw new MoodGenreNotValidException($"Mood with id {id} does not have any seed genres defined.");

            List<Track> tracks = await GetMoodTracks(mood);
            return StatusCode(200, ApiResponse<Track>.SuccessList(HttpStatusCode.OK, tracks));
        }

        // Spotify allows at most five seeds in total. Track seeds are kept first and
        // the remaining slots are shared across the selected moods' configured genres.
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations([FromQuery] RecommendationRequest req)
        {
            var moodIds = req.MoodIds.Distinct().ToList();
            var trackIds = req.TrackIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(NormalizeSpotifyTrackId)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (moodIds.Count == 0 && trackIds.Count == 0)
                throw new RecommendationRequestException("Select at least one mood or Spotify track.");
            if (moodIds.Count + trackIds.Count > 5)
                throw new RecommendationRequestException("Spotify accepts at most five combined mood and track seeds.");

            var moods = moodIds.Count == 0
                ? []
                : await uow.LibraryRepository.GetByIdsAsync(moodIds);
            var missingMoodIds = moodIds.Except(moods.Select(m => m.Id)).ToList();
            if (missingMoodIds.Count > 0)
                throw new RecommendationRequestException(
                    $"The following moods were not found: {string.Join(", ", missingMoodIds)}.");

            var seedGenres = SelectSeedGenres(moods, 5 - trackIds.Count);
            if (seedGenres.Count + trackIds.Count == 0)
                throw new RecommendationRequestException("The selected moods do not have usable Spotify genre seeds.");

            var audioFeatures = BlendAudioFeatures(moods);
            var tracks = await spotifyService.GetRecommendations(
                seedGenres, trackIds, audioFeatures, req.Limit, req.Market);

            return StatusCode(StatusCodes.Status200OK,
                ApiResponse<Track>.SuccessList(HttpStatusCode.OK, tracks));
        }

        private static List<string> SelectSeedGenres(IReadOnlyList<Mood> moods, int availableSlots)
        {
            var genresByMood = moods
                .Select(mood => new Queue<string>(mood.GetSeedGenres()
                    .Distinct(StringComparer.OrdinalIgnoreCase)))
                .ToList();
            var result = new List<string>();

            while (result.Count < availableSlots && genresByMood.Any(queue => queue.Count > 0))
            {
                foreach (var genres in genresByMood)
                {
                    while (genres.Count > 0 && result.Contains(genres.Peek(), StringComparer.OrdinalIgnoreCase))
                        genres.Dequeue();
                    if (genres.Count > 0 && result.Count < availableSlots)
                        result.Add(genres.Dequeue());
                }
            }

            return result;
        }

        private static Dictionary<string, Dictionary<string, double>> BlendAudioFeatures(
            IReadOnlyCollection<Mood> moods)
        {
            return moods
                .SelectMany(mood => mood.GetAudioFeatures())
                .GroupBy(feature => feature.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .SelectMany(feature => feature.Value)
                        .GroupBy(constraint => constraint.Key, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            constraint => constraint.Key,
                            constraint => constraint.Average(value => value.Value)),
                    StringComparer.OrdinalIgnoreCase);
        }

        private static string NormalizeSpotifyTrackId(string value)
        {
            var trimmed = value.Trim();
            const string spotifyPrefix = "spotify:track:";
            if (trimmed.StartsWith(spotifyPrefix, StringComparison.OrdinalIgnoreCase))
                return trimmed[spotifyPrefix.Length..];

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
                uri.Host.Equals("open.spotify.com", StringComparison.OrdinalIgnoreCase))
            {
                var segments = uri.AbsolutePath.Trim('/').Split('/');
                if (segments.Length >= 2 && segments[^2].Equals("track", StringComparison.OrdinalIgnoreCase))
                    return segments[^1];
            }

            return trimmed;
        }

        [HttpPost("playlists")]
        public async Task<IActionResult> CreatePlaylist([FromBody] UpsertPlaylist req)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.CreatePlaylist(userId, req);
            return StatusCode((int)response.StatusCode, response);
        }

        [AllowAnonymous]
        [HttpGet("playlists")]
        public async Task<IActionResult> GetPlaylists(
            [FromQuery] int pageNo = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortDir = "asc",
            [FromQuery] Guid? moodId = null,
            [FromQuery] string? creatorTag = null,
            [FromQuery] string view = "mine")
        {
            Guid? currentUserId = Guid.TryParse(
                HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                out var parsedUserId)
                ? parsedUserId
                : null;

            var normalizedView = view.Trim().ToLowerInvariant();
            if (normalizedView is not ("mine" or "others" or "all"))
                throw new RecommendationRequestException("View must be mine, others, or all.");
            if (normalizedView == "mine" && currentUserId is null)
                throw new UnauthorizedAccessException("Sign in to view your playlists.");

            var ownerId = normalizedView == "mine" ? currentUserId : null;
            var excludedOwnerId = normalizedView == "others" ? currentUserId : null;
            var response = await uow.LibraryRepository.GetPlaylists(
                pageNo, pageSize, sortDir, ownerId, excludedOwnerId, moodId, creatorTag);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("playlists/{playlistId}")]
        public async Task<IActionResult> UpdatePlaylist(
            [FromRoute] Guid playlistId,
            [FromBody] UpsertPlaylist req)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.UpdatePlaylist(userId, playlistId, req);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("playlists/{playlistId}/tracks")]
        public async Task<IActionResult> AddTracks(
            [FromRoute] Guid playlistId,
            [FromBody] SaveTracksRequest req)
        {
            Guid userId = Guid.Parse(GetUserId());
            if (req.Tracks.Count == 0)
                throw new RecommendationRequestException("Select at least one track to save.");

            var response = await uow.LibraryRepository.AddTracksAsync(userId, playlistId, req.Tracks);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("playlists/{playlistId}/refresh")]
        public async Task<IActionResult> RefreshPlaylist([FromRoute] Guid playlistId)
        {
            Guid userId = Guid.Parse(GetUserId());
            Guid? moodId = await uow.LibraryRepository.GetOwnedPlaylistMoodId(userId, playlistId);
            if (moodId is null)
                throw new RecommendationRequestException(
                    "The playlist was not found in your library or does not have a mood.");

            Mood mood = await uow.LibraryRepository.GetByIdAsync(moodId.Value)
                ?? throw new MoodNotFoundException(moodId.Value);
            var tracks = await GetMoodTracks(mood);
            var response = await uow.LibraryRepository.AddTracksAsync(userId, playlistId, tracks);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("playlists/{playlistId}/tracks/{trackId}")]
        public async Task<IActionResult> RemoveTrack(
            [FromRoute] Guid playlistId,
            [FromRoute] string trackId)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.RemoveTrack(userId, playlistId, trackId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("playlists/{playlistId}/tracks/{trackId}/exists")]
        public async Task<IActionResult> TrackExists(
            [FromRoute] Guid playlistId,
            [FromRoute] string trackId)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.ExistsAsync(userId, playlistId, trackId);
            return StatusCode((int)response.StatusCode, response);
        }

        private async Task<List<Track>> GetMoodTracks(Mood mood)
        {
            var tracks = await cacheService.GetOrCreateAsync(
                $"spotify:mood:{mood.Id}",
                () => spotifyService.GetTracksByMoodRecommendations(
                    mood.GetSeedGenres(), mood.GetAudioFeatures()),
                TimeSpan.FromHours(1));

            return tracks ?? [];
        }
    }
}
