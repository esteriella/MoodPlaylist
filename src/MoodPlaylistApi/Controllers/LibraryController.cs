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
    /// <summary>Discover moods and recommendations, and manage personal or community playlists.</summary>
    [Authorize]
    [Route("library")]
    [Produces("application/json")]
    public class LibraryController(
        IUnitOfWork uow,
        ISpotifyService spotifyService,
        ICacheService cacheService) : BaseController
    {
        /// <summary>List the moods available for recommendations and playlist filtering.</summary>
        /// <response code="200">The configured moods.</response>
        [AllowAnonymous]
        [HttpGet("available-moods", Name = "GetAvailableMoods")]
        [ProducesResponseType(typeof(ApiResponse<List<AvailableMood>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableMoods()
        {
            var response = await uow.LibraryRepository.GetAvailableMoods();
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>Discover Spotify tracks for one mood.</summary>
        /// <param name="id">A mood ID returned by the available moods endpoint.</param>
        /// <response code="200">Recommended Spotify tracks. Results are cached for one hour.</response>
        /// <response code="404">The mood was not found or has no genre configuration.</response>
        /// <response code="502">Spotify could not complete the request.</response>
        [HttpGet("available-moods/{id}/tracks", Name = "GetAvailableMoodTracks")]
        [ProducesResponseType(typeof(ApiResponse<List<Track>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAvailableMoodTracks([FromRoute] Guid id)
        {
            Mood mood = await uow.LibraryRepository.GetByIdAsync(id) ?? throw new MoodNotFoundException(id);
            List<string> seedGenres = mood.GetSeedGenres();
            if (seedGenres is { Count: 0})
                throw new MoodGenreNotValidException($"Mood with id {id} does not have any seed genres defined.");

            List<Track> tracks = await GetMoodTracks(mood);
            return StatusCode(200, ApiResponse<Track>.SuccessList(HttpStatusCode.OK, tracks));
        }

        /// <summary>Discover tracks from selected moods, tracks, or both.</summary>
        /// <remarks>
        /// Supply repeated query parameters, for example:
        /// `?moodIds={id1}&amp;moodIds={id2}&amp;trackIds={spotifyTrackId}&amp;limit=20&amp;market=NG`.
        /// Up to five combined mood and track selections can shape the discovery results.
        /// Mood genres are searched directly. Track selections contribute their artists and are excluded from the results.
        /// </remarks>
        /// <param name="req">Recommendation seeds, result limit, and optional market.</param>
        /// <response code="200">Tracks matching the selected seeds.</response>
        /// <response code="400">No seeds were supplied, a mood is missing, or more than five seeds were selected.</response>
        /// <response code="502">Spotify could not complete the request.</response>
        [HttpGet("recommendations", Name = "GetRecommendations")]
        [ProducesResponseType(typeof(ApiResponse<List<Track>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status502BadGateway)]
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
                throw new RecommendationRequestException("Select at most five combined moods and tracks.");

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

        /// <summary>Create a playlist in the signed-in user's library.</summary>
        /// <param name="req">Playlist title, optional mood, and tracks encoded as a JSON array.</param>
        /// <response code="200">The created playlist.</response>
        /// <response code="409">The user already has a playlist with this title.</response>
        [HttpPost("playlists", Name = "CreatePlaylist")]
        [ProducesResponseType(typeof(ApiResponse<UserPlaylist>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreatePlaylist([FromBody] UpsertPlaylist req)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.CreatePlaylist(userId, req);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>Browse personal or community playlists.</summary>
        /// <remarks>
        /// Use `view=mine` for the current user's playlists, `view=others` to exclude them,
        /// or `view=all` for the full gallery. Results can also be filtered by mood or creator tag.
        /// </remarks>
        /// <param name="pageNo">Page number starting at 1.</param>
        /// <param name="pageSize">Maximum playlists to return.</param>
        /// <param name="sortDir">`asc` or `desc`, based on creation time.</param>
        /// <param name="moodId">Optional mood filter.</param>
        /// <param name="creatorTag">Optional public creator tag.</param>
        /// <param name="view">One of `mine`, `others`, or `all`.</param>
        [AllowAnonymous]
        [HttpGet("playlists", Name = "GetPlaylists")]
        [ProducesResponseType(typeof(ApiResponse<List<UserPlaylist>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
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

        /// <summary>Update an owned playlist's title or append tracks.</summary>
        /// <param name="playlistId">The playlist to update.</param>
        /// <param name="req">The new title, mood, or tracks.</param>
        [HttpPut("playlists/{playlistId}", Name = "UpdatePlaylist")]
        [ProducesResponseType(typeof(ApiResponse<UserPlaylist>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePlaylist(
            [FromRoute] Guid playlistId,
            [FromBody] UpsertPlaylist req)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.UpdatePlaylist(userId, playlistId, req);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>Save one or more tracks to an owned playlist.</summary>
        /// <param name="playlistId">The destination playlist.</param>
        /// <param name="req">Tracks to add; duplicates are skipped.</param>
        [HttpPost("playlists/{playlistId}/tracks", Name = "AddTracks")]
        [ProducesResponseType(typeof(ApiResponse<List<Track>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
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

        /// <summary>Add fresh Spotify recommendations based on an owned playlist's mood.</summary>
        /// <param name="playlistId">An owned playlist with a mood.</param>
        [HttpPost("playlists/{playlistId}/refresh", Name = "RefreshPlaylist")]
        [ProducesResponseType(typeof(ApiResponse<List<Track>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status502BadGateway)]
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

        /// <summary>Remove a track from an owned playlist.</summary>
        /// <param name="playlistId">The playlist containing the track.</param>
        /// <param name="trackId">The Spotify track ID.</param>
        [HttpDelete("playlists/{playlistId}/tracks/{trackId}", Name = "RemoveTrack")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveTrack(
            [FromRoute] Guid playlistId,
            [FromRoute] string trackId)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.RemoveTrack(userId, playlistId, trackId);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>Check whether a track exists in an owned playlist.</summary>
        /// <param name="playlistId">The playlist to inspect.</param>
        /// <param name="trackId">The Spotify track ID.</param>
        [HttpGet("playlists/{playlistId}/tracks/{trackId}/exists", Name = "TrackExists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
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
