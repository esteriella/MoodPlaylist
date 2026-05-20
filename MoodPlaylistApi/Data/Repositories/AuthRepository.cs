using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Dtos.Auth;
using MoodPlaylistApi.Helpers;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Utilities;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MoodPlaylistApi.Data.Repositories
{
    public sealed class AuthRepository(AppDbContext dc) : IAuthRepository
    {

        // Implement the RegisterAsync method
        public async Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerRequest)
        {
            // Bad practice will be used here as this isn't a real project and is only for demonstration purposes. 

            if(await dc.Users.AnyAsync(u => u.Email == registerRequest.Email))
            {
                return ApiResponse<LoginResponseDto>.Error(HttpStatusCode.BadRequest, "Email already exists.");
            }
            int count = await dc.Users.CountAsync();
            // Hash the password before saving (this is a simplified example, consider using a proper hashing algorithm in production)
            string passwordHash = HashString(registerRequest.Password);
            // Generate tag for public user identification
            string tag = await CodeGenerator.Generate($"User-{count+1}-");
            var (refreshToken, refreshTokenExpiry) = Jwt.GenerateRefreshToken();
            // Create a new user entity
            User user = new()
            {
                Name = registerRequest.Name,
                Email = registerRequest.Email,
                PasswordHash = passwordHash,
                PublicId = tag,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = refreshTokenExpiry
            };

            await dc.Users.AddAsync(user);
            await dc.SaveChangesAsync();
            string token = Jwt.CreateToken(user);
            return ApiResponse<LoginResponseDto>.Success(HttpStatusCode.Created, "User registered successfully.", new LoginResponseDto
            {
                Name = user.Name,
                Tag = user.PublicId,
                Token = token,
                RefreshToken = refreshToken
            });
        }

        // Implement the LoginAsync method
        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginRequest)
        {
            var user = await dc.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            if(user is null)
            {
                return ApiResponse<LoginResponseDto>.Error(HttpStatusCode.BadRequest, "Invalid email or password.");
            }

            string passwordHash = HashString(loginRequest.Password);

            if(user.PasswordHash != passwordHash)
            {
                return ApiResponse<LoginResponseDto>.Error(HttpStatusCode.BadRequest, "Invalid email or password.");
            }

            var (refreshToken, refreshTokenExpiry) = Jwt.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiry;
            user.LastLoginTime = DateTime.UtcNow;
            dc.Users.Update(user);
            await dc.SaveChangesAsync();
            string token = Jwt.CreateToken(user);

            return ApiResponse<LoginResponseDto>.Success(HttpStatusCode.Created, "User registered successfully.", new LoginResponseDto
            {
                Name = user.Name,
                Tag = user.PublicId,
                Token = token,
                RefreshToken = refreshToken
            });
        }

        private static string HashString(string rawKey)
        {
            var secret = Encoding.UTF8.GetBytes(HashHelperSettings.SecretKey);
            using var hmac = new HMACSHA256(secret);
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawKey));
            return Convert.ToHexString(bytes);
        }

    }
}
