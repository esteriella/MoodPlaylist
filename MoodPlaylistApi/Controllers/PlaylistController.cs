using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Services;

namespace MoodPlaylistApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistController(AppDbContext context, ISpotifyService spotify) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ISpotifyService _spotify = spotify;

        // POST /api/playlists
        [HttpPost]
        public async Task<IActionResult> CreatePlaylist([FromBody] Guid moodId)
        {
            var mood = await _context.Moods.FindAsync(moodId);
            if (mood == null) return NotFound("Mood not found");

            // Call Spotify API to get tracks for this mood
            var spotifyTracksJson = await _spotify.GetTracksForMood(mood.Name);

            var playlist = new Playlist
            {
                Title = $"{mood.Name} Vibes",
                MoodId = mood.Id,
                Tracks = spotifyTracksJson,
                // satisfy required navigation/property initializers
                Mood = mood,
                // If you don't have a User to assign here, use null-forgiving to satisfy the compiler.
                // Replace with an actual User instance or lookup when you have user context.
                User = null!
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            return Ok(playlist);
        }

        // GET /api/playlists/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlaylist(Guid id)
        {
            var playlist = await _context.Playlists
                .Include(p => p.Mood)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (playlist == null) return NotFound();
            return Ok(playlist);
        }
    }
}
