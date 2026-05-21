using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Utilities;
using System.Net;

namespace MoodPlaylistApi.Data.Repositories
{
    public sealed class MoodRepository(AppDbContext dc) : IMoodRepository
    {
        public async Task<ApiResponse<List<AvailableMood>>> GetAvailableMoods()
        {
            var moods = await dc.Moods
            .AsNoTracking()
            .Select(m => new AvailableMood
            {
                Id = m.Id,
                Name = m.Name,
                Color = m.Color,
                Emoji = m.Emoji
            })
            .ToListAsync();
            return ApiResponse<AvailableMood>.SuccessList(HttpStatusCode.OK, data: moods);
        }

        public async Task<IEnumerable<Mood>> GetAllAsync()
        {
            return await _dc.Moods.ToListAsync();
        }

        public async Task<Mood?> GetByIdAsync(Guid id) =>
            await dc.Moods.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

        public async Task AddAsync(Mood mood) =>
            await _dc.Moods.AddAsync(mood);

        public void Update(Mood mood) =>
            _dc.Moods.Update(mood);

        public void Remove(Mood mood) =>
            _dc.Moods.Remove(mood);
    }

}
