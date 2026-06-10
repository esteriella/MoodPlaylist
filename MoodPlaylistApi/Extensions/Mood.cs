using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Models;

namespace MoodPlaylistApi.Extensions
{
    public static class MoodExtensions
    {
        public static List<string> GetSeedGenres(this Mood mood)
        {
            var json = System.Text.Json.JsonDocument.Parse(mood.SeedGenres);
            if (json.RootElement.TryGetProperty("genres", out var genresElement) && genresElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                return [.. genresElement.EnumerateArray().Select(g => g.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s))];
            }
            return [];
        }

        public static Dictionary<string, Dictionary<string, double>> GetAudioFeatures(this Mood mood)
        {
            var json = System.Text.Json.JsonDocument.Parse(mood.AudioFeatures);
            var features = new Dictionary<string, Dictionary<string, double>>();
            foreach (var feature in json.RootElement.EnumerateObject())
            {
                var featureName = feature.Name;
                var constraints = new Dictionary<string, double>();
                foreach (var constraint in feature.Value.EnumerateObject())
                {
                    if (constraint.Value.TryGetDouble(out var value))
                    {
                        constraints[constraint.Name] = value;
                    }
                }
                features[featureName] = constraints;
            }
            return features;
        }

        public static AvailableMood GetAvailableMood(this Mood mood) => new() 
        { 
            Id = mood.Id,
            Name = mood.Name,
            Color = mood.Color,
            Emoji = mood.Emoji
        };
    }
}
