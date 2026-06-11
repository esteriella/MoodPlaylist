using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Models;

namespace MoodPlaylistApi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) 
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Mood> Moods { get; set; }
        public DbSet<Playlist> Playlists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Mood.Init(modelBuilder);
            User.Init(modelBuilder);
            Playlist.Init(modelBuilder);
            /*modelBuilder.Entity<Mood>().HasData(
                new Mood
                {
                    Id = Guid.CreateVersion7(),
                    CreatedAt = DateTime.UtcNow,
                    Name = "Happy",
                    Color = "#FFD700",
                    Emoji = "😊",
                    SeedGenres = "{\"genres\": [\"pop\", \"dance\", \"dancehall\", \"happy\"]}",
                    AudioFeatures = "{\"valence\": {\"min\": 0.7}, \"energy\": {\"min\": 0.6}}"
                },
                new Mood
                {
                    Id = Guid.CreateVersion7(),
                    CreatedAt = DateTime.UtcNow,
                    Name = "Sad",
                    Color = "#1E90FF",
                    Emoji = "😢",
                    SeedGenres = "{\"genres\": [\"sad\", \"acoustic\", \"melancholy\", \"piano\"]}",
                    AudioFeatures = "{\"valence\": {\"max\": 0.3}, \"energy\": {\"max\": 0.4}, \"acousticness\": {\"min\": 0.5}}"
                },
                new Mood
                {
                    Id = Guid.CreateVersion7(),
                    CreatedAt = DateTime.UtcNow,
                    Name = "Energetic",
                    Color = "#FF4500",
                    Emoji = "⚡",
                    SeedGenres = "{\"genres\": [\"rock\", \"hip-hop\", \"workout\", \"edm\"]}",
                    AudioFeatures = "{\"energy\": {\"min\": 0.8}, \"danceability\": {\"min\": 0.7}, \"tempo\": {\"min\": 120}}"
                },
                new Mood
                {
                    Id = Guid.CreateVersion7(),
                    CreatedAt = DateTime.UtcNow,
                    Name = "Relaxed",
                    Color = "#32CD32",
                    Emoji = "🌿",
                    SeedGenres = "{\"genres\": [\"chill\", \"ambient\", \"lo-fi\", \"acoustic\"]}",
                    AudioFeatures = "{\"energy\": {\"max\": 0.5}, \"tempo\": {\"max\": 100}, \"acousticness\": {\"min\": 0.4}}"
                },
                new Mood
                {
                    Id = Guid.CreateVersion7(),
                    CreatedAt = DateTime.UtcNow,
                    Name = "Focused",
                    Color = "#8A2BE2",
                    Emoji = "🎧",
                    SeedGenres = "{\"genres\": [\"classical\", \"jazz\", \"instrumental\"]}",
                    AudioFeatures = "{\"instrumentalness\": {\"min\": 0.7}, \"energy\": {\"target\": 0.5}, \"tempo\": {\"target\": 90}}"
                },
                new Mood
                {
                    Id = Guid.CreateVersion7(),
                    CreatedAt = DateTime.UtcNow,
                    Name = "Romantic",
                    Color = "#FF69B4",
                    Emoji = "💕",
                    SeedGenres = "{\"genres\": [\"romance\", \"r&b\", \"soul\", \"love\"]}",
                    AudioFeatures = "{\"valence\": {\"target\": 0.6}, \"acousticness\": {\"target\": 0.5}, \"energy\": {\"target\": 0.5}}"
                },
                new Mood
                {
                    Id = Guid.CreateVersion7(),
                    CreatedAt = DateTime.UtcNow,
                    Name = "Angry",
                    Color = "#8B0000",
                    Emoji = "😡",
                    SeedGenres = "{\"genres\": [\"metal\", \"hard-rock\", \"punk\"]}",
                    AudioFeatures = "{\"energy\": {\"min\": 0.9}, \"valence\": {\"max\": 0.4}, \"tempo\": {\"min\": 130}}"
                },
                new Mood
                {
                    Id = Guid.CreateVersion7(),
                    CreatedAt = DateTime.UtcNow,
                    Name = "Dreamy",
                    Color = "#00CED1",
                    Emoji = "🌌",
                    SeedGenres = "{\"genres\": [\"dream-pop\", \"shoegaze\", \"ambient\"]}",
                    AudioFeatures = "{\"valence\": {\"target\": 0.5}, \"energy\": {\"max\": 0.5}, \"acousticness\": {\"min\": 0.3}}"
                }
            );*/
        }
    }
}
