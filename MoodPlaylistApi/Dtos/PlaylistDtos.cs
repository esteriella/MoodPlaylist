namespace MoodPlaylistApi.Dtos
{

    public sealed record CreatePlaylist
    {
        public required string Title { get; set; }
        public string Tracks { get; set; } = "{}";
    }
}
