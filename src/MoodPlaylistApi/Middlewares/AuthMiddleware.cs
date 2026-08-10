using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data;
using MoodPlaylistApi.Helpers;
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

                    var publicId = jwtToken.Claims
                        .FirstOrDefault(c => c.Type == CustomClaimTypes.UserId)?.Value;
                    if (!string.IsNullOrWhiteSpace(publicId))
                    {
                        var user = await dc.Users.FirstOrDefaultAsync(u => u.PublicId == publicId);
                        if (user is not null && context.User.Identity is ClaimsIdentity identity)
                        {
                            if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                            if (!identity.HasClaim(c => c.Type == ClaimTypes.Email))
                                identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
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
