using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Middlewares;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Services;
using System.Security.Claims;

namespace MoodPlaylistApi.Tests.Integration;

[Collection("PostgreSQL")]
[Trait("Category", "Integration")]
public sealed class AuthMiddlewareIntegrationTests(PostgreSqlFixture database)
{
    [Fact(DisplayName = "Auth middleware enriches identity claims from PostgreSQL")]
    public async Task Invoke_ValidBearerToken_AddsDatabaseUserClaimsAndContinues()
    {
        await database.ResetDatabaseAsync();
        await using var dbContext = database.CreateContext();
        var user = new User
        {
            Name = "Ada",
            PublicId = "user-123",
            Email = "ada@example.com"
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var nextCalled = false;
        var middleware = new AuthMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: "Test"))
        };
        context.Request.Headers.Authorization = $"Bearer {Jwt.CreateToken(user)}";

        await middleware.Invoke(context, dbContext);

        Assert.True(nextCalled);
        Assert.Equal(user.Id.ToString(), context.User.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal(user.Email, context.User.FindFirstValue(ClaimTypes.Email));
    }

    [Fact(DisplayName = "Auth middleware ignores malformed bearer tokens and continues")]
    public async Task Invoke_MalformedBearerToken_LeavesClaimsUnchangedAndContinues()
    {
        await database.ResetDatabaseAsync();
        await using var dbContext = database.CreateContext();
        var nextCalled = false;
        var middleware = new AuthMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: "Test"))
        };
        context.Request.Headers.Authorization = "Bearer not-a-jwt";

        await middleware.Invoke(context, dbContext);

        Assert.True(nextCalled);
        Assert.Empty(context.User.Claims);
        Assert.Empty(await dbContext.Users.AsNoTracking().ToListAsync());
    }
}
