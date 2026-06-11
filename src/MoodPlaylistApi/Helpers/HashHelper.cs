using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;

namespace MoodPlaylistApi.Helpers
{
    public sealed class HashHelper
    {
        public required string SecretKey { get; init; }
    }

    public static class HashHelperSettings
    {
        public static string SecretKey { get; private set; } = string.Empty;
        public static void Configure(IConfiguration configuration)
        {
            var hashSettings = configuration.GetSection("HashHelper").Get<HashHelper>();

            SecretKey = !string.IsNullOrEmpty(hashSettings?.SecretKey)
                ? hashSettings.SecretKey
                : throw new ArgumentNullException("HashHelper Key cannot be null or empty.",
                    nameof(hashSettings.SecretKey));
        }
    }
}
