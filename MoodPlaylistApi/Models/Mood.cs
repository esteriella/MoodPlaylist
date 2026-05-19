namespace MoodPlaylistApi.Models
{
     public class Mood : BaseEntity
    {
        public required string Name { get; set; }
        public string? Color { get; set; }
        public string? Emoji { get; set; }
        public ICollection<Playlist> Playlists { get; set; } = [];
    }
}
