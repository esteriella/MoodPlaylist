using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistApi.Dtos.Auth
{
    public sealed record RegisterDto
    {
        /// <summary>The display name shown on playlists.</summary>
        /// <example>Ada Lovelace</example>
        [MinLength(2), MaxLength(100)]
        public required string Name { get; set; }
        /// <summary>The unique email used to sign in.</summary>
        /// <example>ada@example.com</example>
        [EmailAddress]
        public required string Email { get; set; }

        /// <summary>An 8–12 character password containing upper and lowercase letters, a number, and a special character.</summary>
        /// <example>Mood#2026</example>
        [DataType(DataType.Password), MinLength(8), MaxLength(12)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and include at least one uppercase letter, one number, and one special character.")]
        public required string Password { get; set; }
    }

    public sealed record LoginDto
    {
        /// <summary>The account email.</summary>
        /// <example>ada@example.com</example>
        [EmailAddress]
        public required string Email { get; set; }

        /// <summary>The account password.</summary>
        /// <example>Mood#2026</example>
        [DataType(DataType.Password), MinLength(8), MaxLength(12)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and include at least one uppercase letter, one number, and one special character.")]
        public required string Password { get; set; }
    }

    public sealed record LoginResponseDto
    {
        public string Tag { get; set; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }

}
