using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoodPlaylistApi.Models
{
    public class Playlist : BaseEntity
    {
        public required string Title { get; set; }
        public Guid? MoodId { get; set; }
        public Guid? UserId { get; set; }
        public string Tracks { get; set; } = "{}";

        public Mood? Mood { get; set; }
        public User? User { get; set; }

        public static void Init(ModelBuilder builder)
        {
            builder.Entity<Playlist>()
                .HasOne(p => p.Mood)
                .WithMany(m => m.Playlists)
                .HasForeignKey(p => p.MoodId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Playlist>()
                .HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Playlist>().Property(x => x.Tracks).HasColumnType("jsonb");

            // Set rule to make sure either MoodId or UserId is present (playlist must be associated with either a mood or a user)
            builder.Entity<Playlist>()
                .ToTable(b => b
                .HasCheckConstraint("CK_Playlist_MoodOrUser", "\"MoodId\" IS NOT NULL OR \"UserId\" IS NOT NULL"));
        }
    }
}
