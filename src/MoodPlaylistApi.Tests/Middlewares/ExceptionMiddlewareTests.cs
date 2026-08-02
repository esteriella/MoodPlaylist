using Microsoft.AspNetCore.Http;
using MoodPlaylistApi.Middlewares;
using MoodPlaylistApi.Tests.TestSupport;
using System.Text.Json;

namespace MoodPlaylistApi.Tests.Middlewares;

public sealed class ExceptionMiddlewareTests
{
    [Fact(DisplayName = "Exception middleware continues a successful request without changing its response")]
    public async Task Invoke_NextCompletes_LeavesResponseUnchanged()
    {
        var nextCalled = false;
        var middleware = new ExceptionMiddleware(
            new ListLogger<ExceptionMiddleware>(),
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
        var context = new DefaultHttpContext();

        await middleware.Invoke(context);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Exception middleware converts downstream exceptions to JSON errors")]
    public async Task Invoke_NextThrows_ReturnsMappedErrorResponse()
    {
        var exception = new UnauthorizedAccessException("internal detail");
        var logger = new ListLogger<ExceptionMiddleware>();
        var middleware = new ExceptionMiddleware(logger, _ => throw exception);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.Invoke(context);

        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(response.RootElement.GetProperty("successful").GetBoolean());
        Assert.Same(exception, Assert.Single(logger.Exceptions));
    }
}
