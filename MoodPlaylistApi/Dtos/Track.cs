using System.Text.Json.Serialization;

namespace MoodPlaylistApi.Dtos
{
    public sealed record SpotifyRecommendationsResponse
    {
        [JsonPropertyName("tracks")]
        public List<Track> Tracks { get; set; } = [];
    }

    public sealed record Track 
    {
        [JsonPropertyName("href")]
        public string HRef { get; set; } = string.Empty;
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("is_playable")]
        public bool Is_Playable { get; set; } = false;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("popularity")]
        public int Popularity { get; set; } = 0;
        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; } = string.Empty;
        [JsonPropertyName("track_number")]
        public int Track_Number { get; set; } = 0;
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
        [JsonPropertyName("is_local")]
        public bool Is_Local { get; set; } = false;
    }
}
