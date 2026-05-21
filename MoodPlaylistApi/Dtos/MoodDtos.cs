namespace MoodPlaylistApi.Dtos
{
    public class MoodDtos
    {
    }

    public sealed record AvailableMood
    {
        public Guid Id { get; init; }
        public required string Name { get; set; }
        public string? Color { get; set; }
        public string? Emoji { get; set; }
    }
}
