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
        public ICollection<Playlist> Playlists { get; set; } = [];
    }
}
