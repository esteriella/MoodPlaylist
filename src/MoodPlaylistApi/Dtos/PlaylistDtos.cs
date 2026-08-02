using MoodPlaylistApi.Models;

namespace MoodPlaylistApi.Dtos
{

    public sealed record UpsertPlaylist
    {
        /// <summary>The playlist title.</summary>
        /// <example>Sunday Reset</example>
        public required string Title { get; set; }
        /// <summary>The optional mood associated with the playlist.</summary>
        /// <example>0198f978-86a4-7de4-8497-9f234edb1520</example>
        public Guid? MoodId { get; set; }
        /// <summary>A JSON array containing the playlist's Spotify tracks.</summary>
        /// <example>[{"id":"4iV5W9uYEdYUVa79Axb7Rh","name":"Sunrise"}]</example>
        public string Tracks { get; set; } = "[]";
    }

    public sealed record SaveTracksRequest
    {
        /// <summary>One or more Spotify tracks to save. Existing track IDs are skipped.</summary>
        public required List<Track> Tracks { get; init; }
    }

    public sealed record UserPlaylist
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public string CreatorTag { get; set; } = string.Empty;
        public AvailableMood? Mood { get; set; }
        public List<Track> Tracks { get; set; } = [];
    }
}
