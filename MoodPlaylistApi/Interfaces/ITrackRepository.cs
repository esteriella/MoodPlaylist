using MoodPlaylistApi.Dtos;

namespace MoodPlaylistApi.Interfaces
{
    public interface ITrackRepository
    {
        Task<IEnumerable<Track>> GetAllAsync();
        Task<Track?> GetByIdAsync(Guid id);
        Task AddAsync(Track track);
        void Update(Track track);
        void Remove(Track track);

        // Custom: get tracks by playlist or mood
        Task<IEnumerable<Track>> GetByPlaylistIdAsync(Guid playlistId);
        Task<IEnumerable<Track>> GetByMoodIdAsync(Guid moodId);
    }

}
