using System.Diagnostics;

namespace MoodPlaylistApi.Middlewares
{
    public sealed class RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            var started = Stopwatch.GetTimestamp();
            var suppliedCorrelationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            var correlationId = !string.IsNullOrWhiteSpace(suppliedCorrelationId) && suppliedCorrelationId.Length <= 100
                ? suppliedCorrelationId
                : context.TraceIdentifier;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            try
            {
                await next(context);
            }
            finally
            {
                var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
                logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds:0.0} ms CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    elapsed,
                    correlationId);
            }
        }
    }
}
