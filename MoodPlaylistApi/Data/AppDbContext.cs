using Microsoft.EntityFrameworkCore;
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

            // Relationships
            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.Mood)
                .WithMany(m => m.Playlists)
                .HasForeignKey(p => p.MoodId);

            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId);
        }
    }
}
