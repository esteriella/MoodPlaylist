using MoodPlaylistApi.Exceptions;

namespace MoodPlaylistApi.Middlewares
{
    public class ExceptionMiddleware(
        ILogger<ExceptionMiddleware> logger,
        RequestDelegate next,
        IConfiguration config)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                var hostEnv = config.GetValue<string>("ASPNETCORE_ENVIRONMENT")
                    ?? throw new InvalidOperationException("Host environment not found.");
                await ExceptionHandler.HandleAsync(context, exception, hostEnv, logger);
            }
        }
    }
}
