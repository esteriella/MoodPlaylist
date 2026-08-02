using MoodPlaylistApi.Exceptions;

namespace MoodPlaylistApi.Middlewares
{
    public class ExceptionMiddleware(
        ILogger<ExceptionMiddleware> logger,
        RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await ExceptionHandler.HandleAsync(context, exception, logger);
            }
        }
    }
}
