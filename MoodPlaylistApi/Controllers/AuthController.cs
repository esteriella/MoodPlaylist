using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Dtos.Auth;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Services;

namespace MoodPlaylistApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IUnitOfWork uow) : ControllerBase
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await uow.AuthRepository.RegisterAsync(dto);

            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await uow.AuthRepository.LoginAsync(dto);

            return StatusCode((int)result.StatusCode, result);
        }
    }

}
