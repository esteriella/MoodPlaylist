using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Utilities;
using System.Net;

namespace MoodPlaylistApi.Controllers
{
    [ApiController]
    [Route("api/playlists")]
    public class PlaylistController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly ISpotifyService _spotify;

        public PlaylistController(IUnitOfWork uow, ISpotifyService spotify)
        {
            _uow = uow;
            _spotify = spotify;
        }

        // 3. Save a playlist based on mood
        [HttpPost]
        public async Task<IActionResult> SavePlaylist([FromBody] CreatePlaylistDto dto)
        {
            var mood = await _uow.Moods.GetByIdAsync(dto.MoodId);
            if (mood == null) return NotFound();

            var tracks = await _spotify.GetRecommendations(mood.SeedGenresJson, mood.AudioFeaturesJson);

            var playlist = new Playlist
            {
                Name = dto.Name,
                UserId = dto.UserId,
                MoodId = dto.MoodId,
                Tracks = tracks.ToList()
            };

            await _uow.Playlists.AddAsync(playlist);
            await _uow.CompleteAsync();

            return Ok(ApiResponse<Playlist>.Success(HttpStatusCode.Created, "Playlist saved", playlist));
        }

        // 4. Update existing playlist
        [HttpPut("{playlistId}")]
        public async Task<IActionResult> UpdatePlaylist(Guid playlistId)
        {
            var playlist = await _uow.Playlists.GetByIdAsync(playlistId);
            if (playlist == null) return NotFound();

            var mood = await _uow.Moods.GetByIdAsync(playlist.MoodId);
            var tracks = await _spotify.GetRecommendations(mood.SeedGenresJson, mood.AudioFeaturesJson);

            playlist.Tracks = tracks.ToList();
            _uow.Playlists.Update(playlist);
            await _uow.CompleteAsync();

            return Ok(ApiResponse<Playlist>.Success(HttpStatusCode.OK, "Playlist updated", playlist));
        }

        // 5. Gallery of playlists
        [HttpGet("gallery")]
        public async Task<IActionResult> GetGallery([FromQuery] string? mood, [FromQuery] string? userId)
        {
            var playlists = await _uow.Playlists.FilterAsync(mood, userId);
            return Ok(ApiResponse<IEnumerable<Playlist>>.Success(HttpStatusCode.OK, "Gallery playlists", playlists));
        }

        // 7. Suggested playlists
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions()
        {
            var playlists = await _uow.Playlists.GetSuggestedAsync();
            return Ok(ApiResponse<IEnumerable<Playlist>>.Success(HttpStatusCode.OK, "Suggested playlists", playlists));
        }
    }

}
