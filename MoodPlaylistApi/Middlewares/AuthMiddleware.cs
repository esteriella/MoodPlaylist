using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data;
using MoodPlaylistApi.Helpers;
using MoodPlaylistApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MoodPlaylistApi.Middlewares
{
    public class AuthMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context, AppDbContext dc)
        {
            // We want to extract user Id context to use and fetch the user email to now attach to claims for use in controllers.
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                var handler = new JwtSecurityTokenHandler();

                try
                {
                    var jwtToken = handler.ReadJwtToken(token);

                    var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
                    if (expClaim is not null && long.TryParse(expClaim.Value, out var expUnix))
                    {
                        var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                        if (expDate < DateTime.UtcNow)
                        {
                            // Token expired → try refresh
                            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.UserId)?.Value;
                            if (userId is not null)
                            {
                                var user = await dc.Users.FirstOrDefaultAsync(u => u.PublicId == userId);
                                if (user is not null)
                                {
                                    if(user.RefreshTokenExpiryTime > DateTime.UtcNow)
                                    {
                                        // Issue new JWT + refresh token
                                        var newJwt = Jwt.CreateToken(user);
                                        var (newRefresh, expiry) = Jwt.GenerateRefreshToken();

                                        user.RefreshToken = newRefresh;
                                        user.RefreshTokenExpiryTime = expiry;
                                        await dc.SaveChangesAsync();

                                        // Attach new tokens to response headers
                                        context.Response.Headers["X-New-JWT"] = newJwt;
                                        context.Response.Headers["X-New-RefreshToken"] = newRefresh;

                                    }

                                    // Attach email claim if not already present
                                    ClaimsIdentity? identity = context.User.Identity as ClaimsIdentity;
                                    if (identity is not null)
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                                        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // If token parsing fails, just continue without modifying claims
                }
            }

            await next(context);
        }
    }
}