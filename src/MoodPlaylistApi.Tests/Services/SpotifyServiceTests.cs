using MoodPlaylistApi.Exceptions;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Tests.TestSupport;
using System.Net;

namespace MoodPlaylistApi.Tests.Services;

public sealed class SpotifyServiceTests
{
    [Fact(DisplayName = "Mood search sends the expected request and returns the response body")]
    public async Task GetTracksForMood_SuccessfulResponse_SendsExpectedRequestAndReturnsContent()
    {
        const string content = "{\"tracks\":{\"items\":[]}}";
        var handler = new RecordingHttpMessageHandler(_ => RecordingHttpMessageHandler.JsonResponse(content));
        var service = CreateService(handler);

        var result = await service.GetTracksForMood("happy");

        Assert.Equal(content, result);
        Assert.Equal(HttpMethod.Get, handler.Request?.Method);
        Assert.Equal("https://spotify.test/v1/search?q=happy&type=track&limit=10", handler.Request?.RequestUri?.AbsoluteUri);
    }

    [Fact(DisplayName = "Mood search throws a Spotify API exception for an unsuccessful response")]
    public async Task GetTracksForMood_UnsuccessfulResponse_ThrowsSpotifyApiException()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse("{\"error\":\"rate limited\"}", HttpStatusCode.TooManyRequests));
        var service = CreateService(handler);

        await Assert.ThrowsAsync<SpotifyApiException>(() => service.GetTracksForMood("happy"));
    }

    [Fact(DisplayName = "Get track sends the expected request and returns the response body")]
    public async Task GetTrackById_SuccessfulResponse_SendsExpectedRequestAndReturnsContent()
    {
        const string content = "{\"id\":\"track-1\",\"name\":\"Sunshine\"}";
        var handler = new RecordingHttpMessageHandler(_ => RecordingHttpMessageHandler.JsonResponse(content));
        var service = CreateService(handler);

        var result = await service.GetTrackById("track-1");

        Assert.Equal(content, result);
        Assert.Equal(HttpMethod.Get, handler.Request?.Method);
        Assert.Equal("https://spotify.test/v1/tracks/track-1", handler.Request?.RequestUri?.AbsoluteUri);
    }

    [Fact(DisplayName = "Get track maps a not found response to TrackNotFoundException")]
    public async Task GetTrackById_NotFoundResponse_ThrowsTrackNotFoundException()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse("{}", HttpStatusCode.NotFound));
        var service = CreateService(handler);

        var exception = await Assert.ThrowsAsync<TrackNotFoundException>(() => service.GetTrackById("missing"));

        Assert.Contains("missing", exception.Message);
    }

    [Fact(DisplayName = "Get track maps other unsuccessful responses to SpotifyApiException")]
    public async Task GetTrackById_OtherUnsuccessfulResponse_ThrowsSpotifyApiException()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse("{}", HttpStatusCode.InternalServerError));
        var service = CreateService(handler);

        await Assert.ThrowsAsync<SpotifyApiException>(() => service.GetTrackById("track-1"));
    }

    [Fact(DisplayName = "Recommendations sends genres and audio features and parses tracks")]
    public async Task GetTracksByMoodRecommendations_SuccessfulResponse_SendsExpectedRequestAndParsesTracks()
    {
        const string content = "{\"tracks\":[{\"id\":\"track-1\",\"name\":\"Sunshine\",\"is_playable\":true}]}";
        var handler = new RecordingHttpMessageHandler(_ => RecordingHttpMessageHandler.JsonResponse(content));
        var service = CreateService(handler);
        var audioFeatures = new Dictionary<string, Dictionary<string, double>>
        {
            ["energy"] = new() { ["target"] = 0.8 },
            ["danceability"] = new() { ["min"] = 0.5 }
        };

        var result = await service.GetTracksByMoodRecommendations(["pop", "dance"], audioFeatures);

        var track = Assert.Single(result);
        Assert.Equal("track-1", track.Id);
        Assert.Equal("Sunshine", track.Name);
        Assert.True(track.IsPlayable);
        Assert.Equal(
            "https://spotify.test/v1/recommendations?seed_genres=pop,dance&limit=20&min_danceability=0.5&target_energy=0.8",
            handler.Request?.RequestUri?.AbsoluteUri);
    }

    [Fact(DisplayName = "Recommendations supports track seeds, market and requested limit")]
    public async Task GetRecommendations_TrackSeedsAndOptions_SendsExpectedRequest()
    {
        var handler = new RecordingHttpMessageHandler(_ => RecordingHttpMessageHandler.JsonResponse("{\"tracks\":[]}"));
        var service = CreateService(handler);

        var result = await service.GetRecommendations(
            [],
            ["track/one", "track-two"],
            new Dictionary<string, Dictionary<string, double>>(),
            35,
            "ng");

        Assert.Empty(result);
        Assert.Equal(
            "https://spotify.test/v1/recommendations?seed_tracks=track%2Fone,track-two&limit=35&market=NG",
            handler.Request?.RequestUri?.AbsoluteUri);
    }

    [Fact(DisplayName = "Recommendations returns an empty list when tracks are absent")]
    public async Task GetTracksByMoodRecommendations_ResponseWithoutTracks_ReturnsEmptyList()
    {
        var handler = new RecordingHttpMessageHandler(_ => RecordingHttpMessageHandler.JsonResponse("{}"));
        var service = CreateService(handler);

        var result = await service.GetTracksByMoodRecommendations(["ambient"], []);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "Recommendations throws a Spotify API exception for an unsuccessful response")]
    public async Task GetTracksByMoodRecommendations_UnsuccessfulResponse_ThrowsSpotifyApiException()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            RecordingHttpMessageHandler.JsonResponse("{}", HttpStatusCode.BadRequest));
        var service = CreateService(handler);

        await Assert.ThrowsAsync<SpotifyApiException>(
            () => service.GetTracksByMoodRecommendations(["unknown"], []));
    }

    private static SpotifyService CreateService(RecordingHttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://spotify.test/") });
}
