using Microsoft.AspNetCore.Authorization;
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
    public class LibraryController(IUnitOfWork uow, ISpotifyService spotifyService) : BaseController
    {
        // Get available moods
        [AllowAnonymous]
        [HttpGet("available-moods")]
        public async Task<IActionResult> GetAvailableMoods()
        {
            var response = await uow.LibraryRepository.GetAvailableMoods();
            return StatusCode((int)response.StatusCode, response);
        }

        // Get tracks for a specific mood from spotify -> is for the user who wants to create a playlist based on mood and not look through playlists created by other users based on the same mood
        // we need to guard this endpoint with imemorycache using cache key moodid so that we don't make too many requests to spotify for the same mood in a short period of time, we can cache the response for a specific mood for a certain amount of time (e.g. 1 hour) and return the cached response if the same mood is requested again within that time frame
        // best to create a central cache service for all caching withing the system
        [HttpGet("available-moods/{id}/tracks")]
        public async Task<IActionResult> GetAvailableMoodTracks([FromRoute] Guid id)
        {
            Mood mood = await uow.LibraryRepository.GetByIdAsync(id) ?? throw new MoodNotFoundException(id);
            // Get tracks for the mood from spotify using recommendations endpoint with the seed genres and audio features from the mood
            List<string> seedGenres = mood.GetSeedGenres();
            if (seedGenres is { Count: 0})
                throw new MoodGenreNotValidException($"Mood with id {id} does not have any seed genres defined.");

            Dictionary<string, Dictionary<string, double>> audioFeatures = mood.GetAudioFeatures();
            List<Track> tracks = await spotifyService.GetTracksByMoodRecommendations(seedGenres, audioFeatures);
            return StatusCode(200, ApiResponse<Track>.SuccessList(HttpStatusCode.OK, tracks));
        }

        // Save a playlist based on mood to the user's library -> is for the user who wants to create a playlist based on mood and save it to their library
        // Create a new playlist
        [HttpPost("playlists")]
        public async Task<IActionResult> CreatePlaylist([FromBody] UpsertPlaylist req)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.CreatePlaylist(userId, req);
            return StatusCode((int)response.StatusCode, response);
        }

        // Update playlist (title and/or tracks)
        [HttpPut("playlists/{playlistId}")]
        public async Task<IActionResult> UpdatePlaylist(
            [FromRoute] Guid playlistId,
            [FromBody] UpsertPlaylist req)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.UpdatePlaylist(userId, playlistId, req);
            return StatusCode((int)response.StatusCode, response);
        }

        // Add a track to a playlist
        [HttpPost("playlists/{playlistId}/tracks")]
        public async Task<IActionResult> AddTrack(
            [FromRoute] Guid playlistId,
            [FromBody] Track track)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.AddTrackAsync(userId, playlistId, track);
            return StatusCode((int)response.StatusCode, response);
        }

        // Remove a track from a playlist
        [HttpDelete("playlists/{playlistId}/tracks/{trackId}")]
        public async Task<IActionResult> RemoveTrack(
            [FromRoute] Guid playlistId,
            [FromRoute] string trackId)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.RemoveTrack(userId, playlistId, trackId);
            return StatusCode((int)response.StatusCode, response);
        }

        // Check if a track exists in a playlist
        [HttpGet("playlists/{playlistId}/tracks/{trackId}/exists")]
        public async Task<IActionResult> TrackExists(
            [FromRoute] Guid playlistId,
            [FromRoute] string trackId)
        {
            Guid userId = Guid.Parse(GetUserId());
            var response = await uow.LibraryRepository.ExistsAsync(userId, playlistId, trackId);
            return StatusCode((int)response.StatusCode, response);
        }

        // Update their own playlists based on mood -> if they already have their own playlist based on a mood, they can update it with new tracks from spotify based on the same mood

        // Gallery of playlists saved by users -> filter based on moods, users, on self created playlists, etc. and they can  save tracks to their library if they like them (they can choose to create a playlist from multiple tracks or save a single track to their specified playlist) -> this is two endpoints


        // Tracks saved to library from gallery of playlists -> if they like a track from a playlist in the gallery, they can save it to their library for easy access later
        // Existing playlists based on app mood suggestions





        // Need a service that resolves userTag to userId

        // Handle self created from frontend by passing the logged in user tag to the query param userTag
    }
}
