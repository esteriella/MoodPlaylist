using System.Net;

namespace MoodPlaylistApi.Tests.TestSupport;

internal sealed class RecordingHttpMessageHandler(
    Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
{
    private int _requestCount;

    public HttpRequestMessage? Request { get; private set; }
    public string? RequestContent { get; private set; }
    public int RequestCount => _requestCount;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _requestCount);
        Request = request;
        RequestContent = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
        return Task.FromResult(responseFactory(request));
    }

    public static HttpResponseMessage JsonResponse(
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK) => new(statusCode)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
        };
}
