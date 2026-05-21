using MoodPlaylistApi.Dtos;

namespace MoodPlaylistApi.Interfaces
{
    public interface ILibraryRepository
    {
        Task<IEnumerable<Track>> GetUserLibraryAsync(Guid userId);
        Task AddTrackAsync(Track track);
        void RemoveTrack(Track track);

        // Optional: check if track already exists in user’s library
        Task<bool> ExistsAsync(Guid userId, Guid trackId);
    }

}
