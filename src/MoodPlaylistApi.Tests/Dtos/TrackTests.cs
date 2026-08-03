using MoodPlaylistApi.Dtos;
using System.Text.Json;

namespace MoodPlaylistApi.Tests.Dtos;

public sealed class TrackTests
{
    [Fact(DisplayName = "Track responses include official Spotify playback links")]
    public void Playback_TrackId_BuildsEmbedAndExternalUrls()
    {
        var track = new Track { Id = "4iV5W9uYEdYUVa79Axb7Rh", Name = "Example" };

        var json = JsonSerializer.Serialize(track, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("https://open.spotify.com/embed/track/4iV5W9uYEdYUVa79Axb7Rh", json);
        Assert.Contains("https://open.spotify.com/track/4iV5W9uYEdYUVa79Axb7Rh", json);
        Assert.Contains("\"playback\"", json);
    }

    [Fact(DisplayName = "Playback links escape unexpected track ID characters")]
    public void Playback_UnsafeCharacters_EncodesPathSegment()
    {
        var playback = new Track { Id = "track/value" }.Playback;

        Assert.Contains("track%2Fvalue", playback.EmbedUrl);
        Assert.Contains("track%2Fvalue", playback.ExternalUrl);
    }
}
