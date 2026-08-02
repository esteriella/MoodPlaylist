using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistApi.Dtos
{
    public sealed record RecommendationRequest
    {
        /// <summary>Mood IDs selected from the available moods endpoint.</summary>
        /// <example>["0198f978-86a4-7de4-8497-9f234edb1520"]</example>
        public List<Guid> MoodIds { get; init; } = [];
        /// <summary>Spotify track IDs, track URIs, or open.spotify.com track URLs used as seeds.</summary>
        /// <example>["4iV5W9uYEdYUVa79Axb7Rh"]</example>
        public List<string> TrackIds { get; init; } = [];

        /// <summary>Maximum number of recommendations to return.</summary>
        /// <example>20</example>
        [Range(1, 100)]
        public int Limit { get; init; } = 20;

        /// <summary>Optional two-letter country code used to determine track availability.</summary>
        /// <example>NG</example>
        [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Market must be a two-letter ISO country code.")]
        public string? Market { get; init; }
    }
}
