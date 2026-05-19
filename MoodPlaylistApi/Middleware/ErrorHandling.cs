using MoodPlaylistApi.Utilities;
using System.Net;
using System.Text.Json;

namespace MoodPlaylistApi.Middleware
{
    public class ErrorHandling
    {
        private readonly RequestDelegate _next;

        public ErrorHandling(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // continue down the pipeline
            }
            catch (Exception ex)
            {
                // log exception here if needed

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = ApiResponse<string>.Error(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred",
                    ex.Message
                );

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
