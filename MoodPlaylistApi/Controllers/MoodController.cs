using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data;
using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Utilities;
using System.Net;

namespace MoodPlaylistApi.Controllers
{
    [ApiController]
    [Route("api/moods")]
    public class MoodController(IUnitOfWork uow, ISpotifyService spotify) : ControllerBase
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ISpotifyService _spotify = spotify;

        // 1. Get available moods
        [HttpGet("moods")]
        public async Task<IActionResult> GetMoods()
        {
            var moods = await _uow.Moods.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Mood>>.Success(HttpStatusCode.OK, "Available moods", moods));
        }

        // 2. Get tracks for a specific mood
        [HttpGet("{moodId}/tracks")]
        public async Task<IActionResult> GetTracks(Guid moodId)
        {
            var mood = await _uow.Moods.GetByIdAsync(moodId);
            if (mood == null) return NotFound();

            var tracks = await _spotify.GetRecommendations(mood.SeedGenresJson, mood.AudioFeaturesJson);
            return Ok(ApiResponse<IEnumerable<Track>>.Success(HttpStatusCode.OK, "Tracks fetched", tracks));
        }
    }


}
