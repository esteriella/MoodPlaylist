using Microsoft.AspNetCore.Mvc;
using MoodPlaylistApi.Services;
using System.Threading.Tasks;

namespace MoodPlaylistApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly Spotify _spotify;

        public PlayerController(Spotify spotify)
        {
            _spotify = spotify;
        }

        // GET /api/player/{trackId}
        [HttpGet("{trackId}")]
        public async Task<IActionResult> GetTrack(string trackId)
        {
            // Spotify.GetTrackById returns Task (no result) according to the provided signatures.
            // Await the operation without assigning to an implicitly-typed variable.
            await _spotify.GetTrackById(trackId, "<your-access-token>");

            // Adjust response as appropriate for your service. Here we return 204 No Content.
            return NoContent();
        }
    }
}
