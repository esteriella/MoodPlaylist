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
        }
    }
}
