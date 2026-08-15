using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data.Repositories;
using MoodPlaylistApi.Dtos.Auth;
using System.Net;

namespace MoodPlaylistApi.Tests.Integration;

[Collection("PostgreSQL")]
[Trait("Category", "Integration")]
public sealed class AuthRepositoryIntegrationTests(PostgreSqlFixture database)
{
    [Fact(DisplayName = "Auth repository registers logs in and logs out against PostgreSQL")]
    public async Task AuthenticationLifecycle_ValidCredentials_PersistsExpectedState()
    {
        await database.ResetDatabaseAsync();
        var registerRequest = new RegisterDto
        {
            Name = "Ada",
            Email = "ada@example.com",
            Password = "Password1!"
        };
        await using var context = database.CreateContext();
        var repository = new AuthRepository(context);

        var registered = await repository.RegisterAsync(registerRequest);
        var loggedIn = await repository.LoginAsync(new LoginDto
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        });
        var user = await context.Users.SingleAsync(x => x.Email == registerRequest.Email);
        var loggedOut = await repository.LogoutAsync(user.Id);

        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(registered.Data?.Token));
        Assert.Equal(HttpStatusCode.OK, loggedIn.StatusCode);
        Assert.Equal("User signed in successfully.", loggedIn.Message);
        Assert.Equal(HttpStatusCode.OK, loggedOut.StatusCode);
        await context.Entry(user).ReloadAsync();
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiryTime);
    }

    [Fact(DisplayName = "Auth repository enforces unique emails and rejects invalid credentials")]
    public async Task RegisterAndLogin_DuplicateEmailOrInvalidPassword_ReturnsBadRequest()
    {
        await database.ResetDatabaseAsync();
        var request = new RegisterDto
        {
            Name = "Ada",
            Email = "ada@example.com",
            Password = "Password1!"
        };
        await using var context = database.CreateContext();
        var repository = new AuthRepository(context);
        await repository.RegisterAsync(request);

        var duplicate = await repository.RegisterAsync(request);
        var invalidLogin = await repository.LoginAsync(new LoginDto
        {
            Email = request.Email,
            Password = "Different1!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Equal("Email already exists.", duplicate.Message);
        Assert.Equal(HttpStatusCode.BadRequest, invalidLogin.StatusCode);
        Assert.Equal("Invalid email or password.", invalidLogin.Message);
    }
}
