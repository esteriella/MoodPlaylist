using Microsoft.AspNetCore.Http.HttpResults;
using MoodPlaylistApi.Utilities;
using System.Net;

namespace MoodPlaylistApi.Exceptions
{
    public static class ExceptionHandler
    {
        const string production = "Production";

        public static async Task HandleAsync(HttpContext context, Exception exception, string env, ILogger logger)
        {
            var exceptionType = exception.GetType();
            HttpStatusCode statusCode;
            string message;
            logger.LogError(exception, "An error occurred while processing a request.");

            if (exceptionType == typeof(UnauthorizedAccessException))
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = "Access denied, authentication is required to access this resource.";
            }
            else if (exceptionType == typeof(BadHttpRequestException))
            {
                statusCode = HttpStatusCode.BadRequest;
                message = "Incorrect request, check your request and try again.";
            }
            else if (exceptionType == typeof(NotFound))
            {
                statusCode = HttpStatusCode.NotFound;
                message = "The resource you are looking for does not exist or has been moved.";
            }
            else if (exceptionType == typeof(ForbidHttpResult))
            {
                statusCode = HttpStatusCode.Forbidden;
                message = "Access denied, you do not have permission to access this resource.";
            }
            else if (exceptionType == typeof(InternalServerError))
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred on our server.";
            }
            else if (exceptionType == typeof(MoodNotFoundException))
            {
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
            }
            else if (exceptionType == typeof(MoodGenreNotValidException))
            {
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
            }
            else if (exceptionType == typeof(SpotifyApiException))
            {
                statusCode = HttpStatusCode.BadGateway;
                message = exception.Message;
            }
            else if (exceptionType == typeof(PlaylistCreationException))
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = exception.Message;
            }
            else
            {
                statusCode = HttpStatusCode.ServiceUnavailable;
                message = "The service is currently unavailable.";
            }
            var response = env.Equals(production, StringComparison.OrdinalIgnoreCase)
                   ? ApiResponse<string>.Error(statusCode, message)
                   : ApiResponse<string>.Error(statusCode, exception.Message, exception.StackTrace);

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
