namespace MoodPlaylistApi.Helpers
{
    public class JwtSettings
    {
        public string? Key { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int MaxAge { get; set; } = 10; // Default value in case it's missing
        public int MaxRefreshAge { get; set; } = 20; // Default value in case it's missing
    }

    public class JwtSettingsHelper
    {
        public static string Key { get; private set; } = string.Empty;
        public static string Issuer { get; private set; } = string.Empty;
        public static string Audience { get; private set; } = string.Empty;
        public static int MaxAge { get; private set; }
        public static int MaxRefreshAge { get; private set; }

        public static void JwtConfigure(IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();

            Key = !string.IsNullOrEmpty(jwtSettings?.Key)
                ? jwtSettings.Key
                : throw new ArgumentNullException("JWT Key cannot be null or empty.",
                    nameof(jwtSettings.Key));

            Issuer = !string.IsNullOrEmpty(jwtSettings?.Issuer)
                ? jwtSettings.Issuer
                : throw new ArgumentNullException("JWT Issuer cannot be null or empty.",
                    nameof(jwtSettings.Issuer));

            Audience = !string.IsNullOrEmpty(jwtSettings?.Audience)
                ? jwtSettings.Audience
                : throw new ArgumentNullException("JWT Audience cannot be null or empty.",
                    nameof(jwtSettings.Audience));

            MaxAge = jwtSettings?.MaxAge > 0
                ? jwtSettings.MaxAge
                : throw new ArgumentException("JWT MaxAge must be greater than 0.",
                    nameof(jwtSettings.MaxAge));

            MaxRefreshAge = jwtSettings?.MaxRefreshAge > 0
                ? jwtSettings.MaxRefreshAge
                : throw new ArgumentException("JWT MaxRefreshAge must be greater than 0.",
                    nameof(jwtSettings.MaxRefreshAge));
        }
    }

    public class CustomClaimTypes
    {
        public const string UserId = "UserId";
        public const string Expires = "Expires";
        public const string Email = "Email";
    }
}
