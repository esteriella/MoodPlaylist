using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Utilities;

namespace MoodPlaylistApi.Interfaces
{
    public interface ILibraryRepository
    {
        Task<ApiResponse<List<AvailableMood>>> GetAvailableMoods();
        Task<Mood?> GetByIdAsync(Guid id);
        Task<List<Mood>> GetByIdsAsync(IReadOnlyCollection<Guid> ids);
        Task<ApiResponse<List<UserPlaylist>>> GetPlaylists(
            int pageNo,
            int pageSize,
            string sortDir,
            Guid? ownerId,
            Guid? excludedOwnerId,
            Guid? moodId,
            string? creatorTag);
        Task<Guid?> GetOwnedPlaylistMoodId(Guid userId, Guid playlistId);
        Task<ApiResponse<UserPlaylist>> CreatePlaylist(Guid userId, UpsertPlaylist req);
        Task<ApiResponse<UserPlaylist>> UpdatePlaylist(Guid userId, Guid playlistId, UpsertPlaylist req);
        Task<ApiResponse<List<Track>>> AddTracksAsync(Guid userId, Guid playlistId, IReadOnlyCollection<Track> tracks);
        Task<ApiResponse<string>> RemoveTrack(Guid userId, Guid playlistId, string trackId);

        // Optional: check if track already exists in user’s library
        Task<ApiResponse<bool>> ExistsAsync(Guid userId, Guid playlistId, string trackId);
    }

}
