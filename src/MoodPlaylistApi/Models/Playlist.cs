using Microsoft.EntityFrameworkCore;

namespace MoodPlaylistApi.Models
{
    public class Playlist : BaseEntity
    {
        public required string Title { get; set; }
        public Guid? MoodId { get; set; }
        public Guid UserId { get; set; }
        public string Tracks { get; set; } = "{}";

        public Mood? Mood { get; set; } 
        public User User { get; set; } = default!;

        public static void Init(ModelBuilder builder)
        {
            builder.Entity<Playlist>().HasIndex(x => new { x.Title, x.UserId }).IsUnique();

            builder.Entity<Playlist>()
                .HasOne(p => p.Mood)
                .WithMany(m => m.Playlists)
                .HasForeignKey(p => p.MoodId)
                .OnDelete(DeleteBehavior.SetNull); // should in case a mood is later deleted in the future

            builder.Entity<Playlist>()
                .HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Playlist>().Property(x => x.Tracks).HasColumnType("jsonb");

            // Set rule to make sure either MoodId or UserId is present (playlist must be associated with either a mood or a user)
            //builder.Entity<Playlist>()
            //    .ToTable(b => b
            //    .HasCheckConstraint("CK_Playlist_MoodOrUser", "\"MoodId\" IS NOT NULL OR \"UserId\" IS NOT NULL"));
        }
    }
}
