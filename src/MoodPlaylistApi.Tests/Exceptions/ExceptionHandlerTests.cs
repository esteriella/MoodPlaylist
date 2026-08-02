using Microsoft.AspNetCore.Http;
using MoodPlaylistApi.Exceptions;
using MoodPlaylistApi.Tests.TestSupport;
using MoodPlaylistApi.Utilities;
using System.Net;
using System.Text.Json;

namespace MoodPlaylistApi.Tests.Exceptions;

public sealed class ExceptionHandlerTests
{
    [Fact(DisplayName = "Spotify failures return a safe message and log internal details")]
    public async Task HandleAsync_SpotifyFailure_HidesInternalDetailsAndLogsException()
    {
        const string sensitiveDetail = "expired token: secret-value";
        var exception = new SpotifyApiException(sensitiveDetail);
        var context = new DefaultHttpContext { TraceIdentifier = "trace-123" };
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/library/recommendations";
        context.Response.Body = new MemoryStream();
        var logger = new ListLogger<object>();

        await ExceptionHandler.HandleAsync(context, exception, logger);

        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        var message = response.RootElement.GetProperty("message").GetString();
        Assert.Equal(StatusCodes.Status502BadGateway, context.Response.StatusCode);
        Assert.Equal("Spotify is temporarily unavailable. Please try again shortly.", message);
        Assert.DoesNotContain(sensitiveDetail, message);
        Assert.Same(exception, Assert.Single(logger.Exceptions));
        Assert.Contains("trace-123", Assert.Single(logger.Messages));
    }

    public static TheoryData<Exception, HttpStatusCode, string> ExceptionMappings => new()
    {
        { new UnauthorizedAccessException(), HttpStatusCode.Unauthorized, "Access denied, authentication is required to access this resource." },
        { new BadHttpRequestException("invalid"), HttpStatusCode.BadRequest, "Incorrect request, check your request and try again." },
        { new MoodNotFoundException(Guid.Empty), HttpStatusCode.NotFound, "Mood with ID 00000000-0000-0000-0000-000000000000 was not found." },
        { new TrackNotFoundException("missing"), HttpStatusCode.NotFound, "Track with ID missing was not found." },
        { new MoodGenreNotValidException("No genres"), HttpStatusCode.NotFound, "No genres" },
        { new RecommendationRequestException("Invalid seeds"), HttpStatusCode.BadRequest, "Invalid seeds" },
        { new PlaylistCreationException("database detail"), HttpStatusCode.InternalServerError, "The playlist could not be created. Please try again." },
        { new InvalidOperationException("internal detail"), HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again." }
    };

    [Theory(DisplayName = "Exception handler maps known failures to the API error contract")]
    [MemberData(nameof(ExceptionMappings))]
    public async Task HandleAsync_ExceptionThrown_ReturnsExpectedErrorContract(
        Exception exception,
        HttpStatusCode expectedStatus,
        string expectedMessage)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = new ListLogger<object>();

        await ExceptionHandler.HandleAsync(context, exception, logger);

        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        var root = response.RootElement;
        Assert.Equal((int)expectedStatus, context.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        Assert.Equal((int)expectedStatus, root.GetProperty("statusCode").GetInt32());
        Assert.False(root.GetProperty("successful").GetBoolean());
        Assert.Equal(expectedMessage, root.GetProperty("message").GetString());
        Assert.Same(exception, Assert.Single(logger.Exceptions));
    }
}
