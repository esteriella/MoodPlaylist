using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Utilities;

namespace MoodPlaylistApi.Interfaces
{
    public interface ILibraryRepository
    {
        Task<ApiResponse<List<AvailableMood>>> GetAvailableMoods();
        Task<Mood?> GetByIdAsync(Guid id);
        Task<ApiResponse<List<UserPlaylist>>> GetUserPlaylists(int pageNo = 1, int pageSize = 10, string sortDir = "asc", Guid? userId = null, Guid? moodId = null);
        Task<ApiResponse<UserPlaylist>> CreatePlaylist(Guid userId, UpsertPlaylist req);
        Task<ApiResponse<UserPlaylist>> UpdatePlaylist(Guid userId, Guid playlistId, UpsertPlaylist req);
        Task<ApiResponse<Track>> AddTrackAsync(Guid userId, Guid playlistId, Track track);
        Task<ApiResponse<string>> RemoveTrack(Guid userId, Guid playlistId, string trackId);

        // Optional: check if track already exists in user’s library
        Task<ApiResponse<bool>> ExistsAsync(Guid userId, Guid playlistId, string trackId);
    }

}
