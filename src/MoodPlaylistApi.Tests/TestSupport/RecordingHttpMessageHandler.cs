using System.Net;

namespace MoodPlaylistApi.Tests.TestSupport;

internal sealed class RecordingHttpMessageHandler(
    Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
{
    public HttpRequestMessage? Request { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Request = request;
        return Task.FromResult(responseFactory(request));
    }

    public static HttpResponseMessage JsonResponse(
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK) => new(statusCode)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
        };
}
