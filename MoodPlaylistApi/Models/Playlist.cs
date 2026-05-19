namespace MoodPlaylistApi.Models
{
    public class Playlist : BaseEntity
    {
        public required string Title { get; set; }
        public Guid? MoodId { get; set; }
        public Guid? UserId { get; set; }
        public string TracksJson { get; set; } = "{}";

        public Mood? Mood { get; set; } 
        public User? User { get; set; } 
    }
}
