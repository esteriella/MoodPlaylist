namespace MoodPlaylistApi.Tests.TestSupport;

internal sealed class AsyncRecordingHttpMessageHandler(
    Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) => responseFactory(request);
}
