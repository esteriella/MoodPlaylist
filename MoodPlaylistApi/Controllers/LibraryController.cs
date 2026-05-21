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
    [ApiController]
    [Route("api/library")]
    public class LibraryController(IUnitOfWork uow, ISpotifyService spotifyService) : Controller
    {
        // Get available moods
        [HttpGet("available-moods")]
        public async Task<IActionResult> GetAvailableMoods()
        {
            var response = await uow.Moods.GetAvailableMoods();
            return StatusCode((int)response.StatusCode, response);
        }

        // Get tracks for a specific mood from spotify -> is for the user who wants to create a playlist based on mood and not look through playlists created by other users based on the same mood
        // we need to guard this endpoint with imemorycache using cache key moodid so that we don't make too many requests to spotify for the same mood in a short period of time, we can cache the response for a specific mood for a certain amount of time (e.g. 1 hour) and return the cached response if the same mood is requested again within that time frame
        // best to create a central cache service for all caching withing the system
        [HttpGet("available-moods/{id}/tracks")]
        public async Task<IActionResult> GetAvailableMoodTracks([FromRoute] Guid id)
        {
            Mood mood = await uow.Moods.GetByIdAsync(id) ?? throw new MoodNotFoundException(id);
            // Get tracks for the mood from spotify using recommendations endpoint with the seed genres and audio features from the mood
            List<string> seedGenres = mood.GetSeedGenres();
            if (seedGenres is { Count: 0})
                throw new MoodGenreNotValidException($"Mood with id {id} does not have any seed genres defined.");

            Dictionary<string, Dictionary<string, double>> audioFeatures = mood.GetAudioFeatures();
            List<Track> tracks = await spotifyService.GetTracksByMoodRecommendations(seedGenres, audioFeatures);
            return StatusCode(200, ApiResponse<Track>.SuccessList(HttpStatusCode.OK, tracks));
        }

        // Save a playlist based on mood to the user's library -> is for the user who wants to create a playlist based on mood and save it to their library
        // Update their own playlists based on mood -> if they already have their own playlist based on a mood, they can update it with new tracks from spotify based on the same mood

        // Gallery of playlists saved by users -> filter based on mood, user, on self created playlists, etc. and they can  save tracks to their library if they like them
        // Tracks saved to library from gallery of playlists -> if they like a track from a playlist in the gallery, they can save it to their library for easy access later
        // Existing playlists based on app mood suggestions
    }
}
