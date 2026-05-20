using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Models;
using System.Text.Json;

namespace MoodPlaylistApi.Extensions
{
    public static class PlaylistExtensions
    {
        public static List<Track> GetTracks(this Playlist playlist)
        {
            try
            {
                return JsonSerializer.Deserialize<List<Track>>(playlist.Tracks) ?? [];
            }
            catch
            {
                return [];
            }
        }
    }
}
