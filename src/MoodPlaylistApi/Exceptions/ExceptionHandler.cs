using Microsoft.AspNetCore.Http.HttpResults;
using MoodPlaylistApi.Utilities;
using System.Net;

namespace MoodPlaylistApi.Exceptions
{
    public static class ExceptionHandler
    {
        public static async Task HandleAsync(HttpContext context, Exception exception, ILogger logger)
        {
            HttpStatusCode statusCode;
            string message;
            logger.LogError(
                exception,
                "Request failed. {Method} {Path} TraceId={TraceId}",
                context.Request.Method,
                context.Request.Path.Value,
                context.TraceIdentifier);

            if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = "Access denied, authentication is required to access this resource.";
            }
            else if (exception is BadHttpRequestException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = "Incorrect request, check your request and try again.";
            }
            else if (exception is MoodNotFoundException or TrackNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
            }
            else if (exception is MoodGenreNotValidException)
            {
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
            }
            else if (exception is RecommendationRequestException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
            }
            else if (exception is SpotifyApiException)
            {
                statusCode = HttpStatusCode.BadGateway;
                message = "Spotify is temporarily unavailable. Please try again shortly.";
            }
            else if (exception is PlaylistCreationException)
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = "The playlist could not be created. Please try again.";
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred. Please try again.";
            }
            var response = ApiResponse<string>.Error(statusCode, message);

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
