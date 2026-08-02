using MoodPlaylistApi.Models;

namespace MoodPlaylistApi.Dtos
{

    public sealed record UpsertPlaylist
    {
        public required string Title { get; set; }
        public Guid? MoodId { get; set; }
        public string Tracks { get; set; } = "[]";
    }

    public sealed record SaveTracksRequest
    {
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
