using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Utilities;

namespace MoodPlaylistApi.Interfaces
{
    public interface IMoodRepository
    {
        Task<ApiResponse<List<AvailableMood>>> GetAvailableMoods();
        Task<IEnumerable<Mood>> GetAllAsync();
        Task<Mood?> GetByIdAsync(Guid id);
        Task AddAsync(Mood mood);
        void Update(Mood mood);
        void Remove(Mood mood);
    }

}
