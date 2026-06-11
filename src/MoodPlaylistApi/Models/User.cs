using Microsoft.EntityFrameworkCore;

namespace MoodPlaylistApi.Models
{
    public class User : BaseEntity
    {
        public required string Name { get; set; }
        public required string PublicId { get; set; }
        public required string Email { get; set; } 
        public string? PasswordHash { get; set; }

        // Refresh token support
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public ICollection<Playlist> Playlists { get; set; } = [];

        public static void Init(ModelBuilder builder)
        {
            builder.Entity<User>().HasIndex(x => x.PublicId).IsUnique();
            builder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        }
    }
}
