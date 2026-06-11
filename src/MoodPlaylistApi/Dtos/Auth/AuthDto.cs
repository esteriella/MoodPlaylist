using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistApi.Dtos.Auth
{
    public sealed record RegisterDto
    {
        [MinLength(2), MaxLength(100)]
        public required string Name { get; set; }
        [EmailAddress]
        public required string Email { get; set; }

        [DataType(DataType.Password), MinLength(8), MaxLength(12)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and include at least one uppercase letter, one number, and one special character.")]
        public required string Password { get; set; }
    }

    public sealed record LoginDto
    {
        [EmailAddress]
        public required string Email { get; set; }

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
