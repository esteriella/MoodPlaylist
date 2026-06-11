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
        public bool IsPlayable { get; set; } = false;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("popularity")]
        public int Popularity { get; set; } = 0;
        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; } = string.Empty;
        [JsonPropertyName("track_number")]
        public int TrackNumber { get; set; } = 0;
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
        [JsonPropertyName("is_local")]
        public bool IsLocal { get; set; } = false;     }

    //public sealed record TrackDetail
    //{
    //    [JsonPropertyName("href")]
    //    public string HRef { get; set; } = string.Empty;
    //    [JsonPropertyName("id")]
    //    public string Id { get; set; } = string.Empty;
    //    [JsonPropertyName("is_playable")]
    //    public bool Is_Playable { get; set; } = false;
    //    [JsonPropertyName("name")]
    //    public string Name { get; set; } = string.Empty;
    //    [JsonPropertyName("popularity")]
    //    public int Popularity { get; set; } = 0;
    //    [JsonPropertyName("preview_url")]
    //    public string PreviewUrl { get; set; } = string.Empty;
    //    [JsonPropertyName("track_number")]
    //    public int Track_Number { get; set; } = 0;
    //    [JsonPropertyName("type")]
    //    public string Type { get; set; } = string.Empty;
    //    [JsonPropertyName("uri")]
    //    public string Uri { get; set; } = string.Empty;
    //    [JsonPropertyName("is_local")]
    //    public bool Is_Local { get; set; } = false;
    //}


    //public class Rootobject
    //{
    //    public Album album { get; set; }
    //    public Artist1[] artists { get; set; }
    //    public string[] available_markets { get; set; }
    //    public int disc_number { get; set; }
    //    public int duration_ms { get; set; }
    //    public bool _explicit { get; set; }
    //    public External_Ids external_ids { get; set; }
    //    public External_Urls2 external_urls { get; set; }
    //    public string href { get; set; }
    //    public string id { get; set; }
    //    public bool is_playable { get; set; }
    //    public Linked_From linked_from { get; set; }
    //    public Restrictions1 restrictions { get; set; }
    //    public string name { get; set; }
    //    public int popularity { get; set; }
    //    public string preview_url { get; set; }
    //    public int track_number { get; set; }
    //    public string type { get; set; }
    //    public string uri { get; set; }
    //    public bool is_local { get; set; }
    //}

    //public class Album
    //{
    //    public string album_type { get; set; }
    //    public int total_tracks { get; set; }
    //    public string[] available_markets { get; set; }
    //    public External_Urls external_urls { get; set; }
    //    public string href { get; set; }
    //    public string id { get; set; }
    //    public Image[] images { get; set; }
    //    public string name { get; set; }
    //    public string release_date { get; set; }
    //    public string release_date_precision { get; set; }
    //    public Restrictions restrictions { get; set; }
    //    public string type { get; set; }
    //    public string uri { get; set; }
    //    public Artist[] artists { get; set; }
    //}

    //public class External_Urls
    //{
    //    public string spotify { get; set; }
    //}

    //public class Restrictions
    //{
    //    public string reason { get; set; }
    //}

    //public class Image
    //{
    //    public string url { get; set; }
    //    public int height { get; set; }
    //    public int width { get; set; }
    //}

    //public class Artist
    //{
    //    public External_Urls1 external_urls { get; set; }
    //    public string href { get; set; }
    //    public string id { get; set; }
    //    public string name { get; set; }
    //    public string type { get; set; }
    //    public string uri { get; set; }
    //}

    //public class External_Urls1
    //{
    //    public string spotify { get; set; }
    //}

    //public class External_Ids
    //{
    //    public string isrc { get; set; }
    //    public string ean { get; set; }
    //    public string upc { get; set; }
    //}

    //public class External_Urls2
    //{
    //    public string spotify { get; set; }
    //}

    //public class Linked_From
    //{
    //}

    //public class Restrictions1
    //{
    //    public string reason { get; set; }
    //}

    //public class Artist1
    //{
    //    public External_Urls3 external_urls { get; set; }
    //    public string href { get; set; }
    //    public string id { get; set; }
    //    public string name { get; set; }
    //    public string type { get; set; }
    //    public string uri { get; set; }
    //}

    //public class External_Urls3
    //{
    //    public string spotify { get; set; }
    //}

}
