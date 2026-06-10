using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MoodPlaylistApi.Startup
{
    public static class AuthDI
    {
        public static void AddJwt(this WebApplicationBuilder builder)
        {
            var issuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Auth authority not configured.");
            var audience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Auth audience not configured.");
            var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Auth key not configured.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };

                options.Authority = issuer;
                options.Audience = audience;

                // Allow HTTP for local development
                if (builder.Environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }
                else
                {
                    options.RequireHttpsMetadata = true;
                }
            });
        }
    }
}
