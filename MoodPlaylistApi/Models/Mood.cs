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
            builder.Entity<Mood>().HasData(
                new Mood
                {
                    Name = "Happy",
                    Color = "#FFD700",
                    Emoji = "😊",
                    SeedGenres = "{\"genres\": [\"pop\", \"dance\", \"dancehall\", \"happy\"]}",
                    AudioFeatures = "{\"valence\": {\"min\": 0.7}, \"energy\": {\"min\": 0.6}}"
                },
                new Mood
                {
                    Name = "Sad",
                    Color = "#1E90FF",
                    Emoji = "😢",
                    SeedGenres = "{\"genres\": [\"sad\", \"acoustic\", \"melancholy\", \"piano\"]}",
                    AudioFeatures = "{\"valence\": {\"max\": 0.3}, \"energy\": {\"max\": 0.4}, \"acousticness\": {\"min\": 0.5}}"
                },
                new Mood
                {
                    Name = "Energetic",
                    Color = "#FF4500",
                    Emoji = "⚡",
                    SeedGenres = "{\"genres\": [\"rock\", \"hip-hop\", \"workout\", \"edm\"]}",
                    AudioFeatures = "{\"energy\": {\"min\": 0.8}, \"danceability\": {\"min\": 0.7}, \"tempo\": {\"min\": 120}}"
                },
                new Mood
                {
                    Name = "Relaxed",
                    Color = "#32CD32",
                    Emoji = "🌿",
                    SeedGenres = "{\"genres\": [\"chill\", \"ambient\", \"lo-fi\", \"acoustic\"]}",
                    AudioFeatures = "{\"energy\": {\"max\": 0.5}, \"tempo\": {\"max\": 100}, \"acousticness\": {\"min\": 0.4}}"
                },
                new Mood
                {
                    Name = "Focused",
                    Color = "#8A2BE2",
                    Emoji = "🎧",
                    SeedGenres = "{\"genres\": [\"classical\", \"jazz\", \"instrumental\"]}",
                    AudioFeatures = "{\"instrumentalness\": {\"min\": 0.7}, \"energy\": {\"target\": 0.5}, \"tempo\": {\"target\": 90}}"
                },
                new Mood
                {
                    Name = "Romantic",
                    Color = "#FF69B4",
                    Emoji = "💕",
                    SeedGenres = "{\"genres\": [\"romance\", \"r&b\", \"soul\", \"love\"]}",
                    AudioFeatures = "{\"valence\": {\"target\": 0.6}, \"acousticness\": {\"target\": 0.5}, \"energy\": {\"target\": 0.5}}"
                },
                new Mood
                {
                    Name = "Angry",
                    Color = "#8B0000",
                    Emoji = "😡",
                    SeedGenres = "{\"genres\": [\"metal\", \"hard-rock\", \"punk\"]}",
                    AudioFeatures = "{\"energy\": {\"min\": 0.9}, \"valence\": {\"max\": 0.4}, \"tempo\": {\"min\": 130}}"
                },
                new Mood
                {
                    Name = "Dreamy",
                    Color = "#00CED1",
                    Emoji = "🌌",
                    SeedGenres = "{\"genres\": [\"dream-pop\", \"shoegaze\", \"ambient\"]}",
                    AudioFeatures = "{\"valence\": {\"target\": 0.5}, \"energy\": {\"max\": 0.5}, \"acousticness\": {\"min\": 0.3}}"
                }
            );

        }
    }
}
