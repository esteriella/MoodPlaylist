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

        public static List<Track> GetTracks(string tracks)
        {
            try
            {
                return JsonSerializer.Deserialize<List<Track>>(tracks) ?? [];
            }
            catch
            {
                return [];
            }
        }

        public static string SetTracks(List<Track> tracks)
        {
            try
            {
                return JsonSerializer.Serialize(tracks);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
