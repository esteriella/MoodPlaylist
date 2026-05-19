using Microsoft.AspNetCore.Mvc;

namespace MoodPlaylistApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet("callback")]
        public IActionResult Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Authorization code missing");

            // Exchange code for access token
            // Call Spotify’s /api/token endpoint here

            return Ok(new { message = "Authorization successful", code });
        }
    }

}
