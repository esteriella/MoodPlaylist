using Microsoft.IdentityModel.Tokens;
using MoodPlaylistApi.Helpers;
using MoodPlaylistApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MoodPlaylistApi.Services
{
    public class Jwt
    {
        public static string CreateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettingsHelper.Key));

            if (key.KeySize < 256)
                throw new InvalidOperationException("JWT key must be at least 256 bits (32 characters).");

            var maxAge = DateTime.UtcNow.AddMinutes(JwtSettingsHelper.MaxAge);

            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(user.PublicId))
                claims.Add(new Claim(CustomClaimTypes.UserId, user.PublicId));

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            claims.Add(new Claim(CustomClaimTypes.Expires, maxAge.ToString()));

            claims.Add(new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(maxAge).ToUnixTimeSeconds().ToString()));

            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()));

            claims.Add(new Claim(JwtRegisteredClaimNames.Iss, JwtSettingsHelper.Issuer));

            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, JwtSettingsHelper.Audience));

            var signingCredentials = new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = maxAge,
                Issuer = JwtSettingsHelper.Issuer,
                Audience = JwtSettingsHelper.Audience,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static (string refreshToken, DateTime expiry) GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var refreshToken = Convert.ToBase64String(randomBytes);
            var expiry = DateTime.UtcNow.AddHours(JwtSettingsHelper.MaxRefreshAge);

            return (refreshToken, expiry);
        }
    }
}
