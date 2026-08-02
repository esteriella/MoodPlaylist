using Microsoft.AspNetCore.Http;
using MoodPlaylistApi.Middlewares;
using MoodPlaylistApi.Tests.TestSupport;

namespace MoodPlaylistApi.Tests.Middlewares;

public sealed class RequestLoggingMiddlewareTests
{
    [Fact(DisplayName = "Request logging records request summary without query or authorization values")]
    public async Task Invoke_RequestCompleted_LogsSafeSummaryAndCorrelationId()
    {
        var logger = new ListLogger<RequestLoggingMiddleware>();
        var middleware = new RequestLoggingMiddleware(
            context =>
            {
                context.Response.StatusCode = StatusCodes.Status201Created;
                return Task.CompletedTask;
            },
            logger);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/library/playlists";
        context.Request.QueryString = new QueryString("?token=secret-query-value");
        context.Request.Headers.Authorization = "Bearer secret-token";
        context.Request.Headers["X-Correlation-ID"] = "request-123";

        await middleware.Invoke(context);

        var message = Assert.Single(logger.Messages);
        Assert.Contains("POST", message);
        Assert.Contains("/library/playlists", message);
        Assert.Contains("201", message);
        Assert.Contains("request-123", message);
        Assert.DoesNotContain("secret-query-value", message);
        Assert.DoesNotContain("secret-token", message);
        Assert.Equal("request-123", context.Response.Headers["X-Correlation-ID"]);
    }
}
