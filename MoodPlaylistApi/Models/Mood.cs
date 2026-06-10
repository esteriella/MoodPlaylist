using Microsoft.EntityFrameworkCore;

namespace MoodPlaylistApi.Models
{
    public class Mood : BaseEntity
    {
        public required string Name { get; set; }
        public string? Color { get; set; }
        public string? Emoji { get; set; }
        // Multiple seeded genres stored as JSON string
        public string SeedGenres { get; set; } = "{}";

        // Audio feature ranges stored as JSON string
        public string AudioFeatures { get; set; } = "{}";
        public ICollection<Playlist> Playlists { get; set; } = [];

        public static void Init(ModelBuilder builder)
        {            
            builder.Entity<Mood>().HasIndex(m => m.Name).IsUnique();
            builder.Entity<Mood>().Property(x => x.SeedGenres).HasColumnType("jsonb");
            builder.Entity<Mood>().Property(x => x.AudioFeatures).HasColumnType("jsonb");
            

        }
    }
}
