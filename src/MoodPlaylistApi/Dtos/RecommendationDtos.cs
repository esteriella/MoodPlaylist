using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistApi.Dtos
{
    public sealed record RecommendationRequest
    {
        public List<Guid> MoodIds { get; init; } = [];
        public List<string> TrackIds { get; init; } = [];

        [Range(1, 100)]
        public int Limit { get; init; } = 20;

        [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Market must be a two-letter ISO country code.")]
        public string? Market { get; init; }
    }
}
