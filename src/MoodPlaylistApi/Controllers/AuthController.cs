using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodPlaylistApi.Dtos.Auth;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Utilities;

namespace MoodPlaylistApi.Controllers
{
    /// <summary>
    /// Controller for handling authentication-related operations such as registration, login, and logout.
    /// </summary>
    /// <param name="uow"></param>
    [Route("auth")]
    [Produces("application/json")]
    public class AuthController(IUnitOfWork uow) : BaseController
    {

        //[HttpGet("callback")]
        //public IActionResult Callback([FromQuery] string code, [FromQuery] string state)
        //{
        //    if (string.IsNullOrEmpty(code))
        //        return BadRequest("Authorization code missing");

        //    // Exchange code for access token
        //    // Call Spotify’s /api/token endpoint here

        //    return Ok(new { message = "Authorization successful", code });
        //}

        /// <summary>Create a MoodPlaylist account.</summary>
        /// <remarks>Creates the account and immediately returns the JWT and refresh token required for authenticated routes.</remarks>
        /// <param name="dto">The new user's name, email address, and password.</param>
        /// <response code="201">The account was created and authentication tokens were issued.</response>
        /// <response code="400">The email is already registered.</response>
        /// <response code="422">One or more fields failed validation.</response>
        [HttpPost("register", Name = "Register")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status201Created, Description = "Account created.")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest, Description = "Email already exists.")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status422UnprocessableEntity, Description = "Validation failed.")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await uow.AuthRepository.RegisterAsync(dto);

            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>Sign in to MoodPlaylist.</summary>
        /// <param name="dto">The account email and password.</param>
        /// <response code="201">Credentials were accepted and fresh tokens were issued.</response>
        /// <response code="400">The email or password is incorrect.</response>
        [HttpPost("login", Name = "Login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status201Created, Description = "Signed in successfully.")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest, Description = "Invalid credentials.")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await uow.AuthRepository.LoginAsync(dto);

            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>Sign out and revoke the current refresh token.</summary>
        /// <response code="200">The session was ended.</response>
        /// <response code="401">A valid JWT was not supplied.</response>
        [Authorize]
        [HttpPost("logout", Name = "Logout")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK, Description = "Signed out successfully.")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized, Description = "Authentication is required.")]
        public async Task<IActionResult> Logout()
        {
            Guid userId = Guid.Parse(GetUserId());
            var result = await uow.AuthRepository.LogoutAsync(userId);

            return StatusCode((int)result.StatusCode, result);
        }
    }

}
