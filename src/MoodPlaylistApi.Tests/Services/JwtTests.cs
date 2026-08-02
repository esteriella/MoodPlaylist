using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoodPlaylistApi.Helpers;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Services;
using System.IdentityModel.Tokens.Jwt;

namespace MoodPlaylistApi.Tests.Services;

[Collection("JwtSettings")]
public sealed class JwtTests
{
    [Fact(DisplayName = "JWT creation includes identity claims and configured token metadata")]
    public void CreateToken_ValidUser_CreatesSignedTokenWithExpectedClaims()
    {
        ConfigureJwt();
        var user = new User
        {
            Name = "Ada",
            PublicId = "user-123",
            Email = "ada@example.com"
        };

        var encodedToken = Jwt.CreateToken(user);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(encodedToken);

        Assert.Equal("issuer.test", token.Issuer);
        Assert.Contains("audience.test", token.Audiences);
        Assert.Equal(SecurityAlgorithms.HmacSha256, token.SignatureAlgorithm);
        Assert.Equal("user-123", token.Claims.Single(x => x.Type == CustomClaimTypes.UserId).Value);
        Assert.Equal("ada@example.com", token.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.False(string.IsNullOrWhiteSpace(token.Id));
        Assert.InRange(token.ValidTo, DateTime.UtcNow.AddMinutes(4), DateTime.UtcNow.AddMinutes(6));
    }

    [Fact(DisplayName = "JWT creation rejects signing keys shorter than 256 bits")]
    public void CreateToken_SigningKeyIsTooShort_ThrowsInvalidOperationException()
    {
        ConfigureJwt(key: "short-key");
        var user = new User { Name = "Ada", PublicId = "user-123", Email = "ada@example.com" };

        var exception = Assert.Throws<InvalidOperationException>(() => Jwt.CreateToken(user));

        Assert.Contains("at least 256 bits", exception.Message);
        ConfigureJwt();
    }

    [Fact(DisplayName = "Refresh token generation creates random 256-bit tokens with configured expiry")]
    public void GenerateRefreshToken_CalledTwice_ReturnsDistinctTokensAndConfiguredExpiry()
    {
        ConfigureJwt();
        var before = DateTime.UtcNow.AddHours(23).AddMinutes(59);

        var first = Jwt.GenerateRefreshToken();
        var second = Jwt.GenerateRefreshToken();

        Assert.NotEqual(first.refreshToken, second.refreshToken);
        Assert.Equal(32, Convert.FromBase64String(first.refreshToken).Length);
        Assert.InRange(first.expiry, before, DateTime.UtcNow.AddHours(24).AddMinutes(1));
    }

    private static void ConfigureJwt(
        string key = "this-is-a-test-signing-key-with-at-least-32-characters")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = "issuer.test",
                ["Jwt:Audience"] = "audience.test",
                ["Jwt:MaxAge"] = "5",
                ["Jwt:MaxRefreshAge"] = "24"
            })
            .Build();
        JwtSettingsHelper.JwtConfigure(configuration);
    }
}

[CollectionDefinition("JwtSettings", DisableParallelization = true)]
public sealed class JwtSettingsCollection;
