using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Interfaces;

namespace MoodPlaylistApi.Data.Repositories
{
    public sealed class LibraryRepository : ILibraryRepository
    {
        private readonly AppDbContext _dc;

        public LibraryRepository(AppDbContext dc) => _dc = dc;

        public async Task<IEnumerable<Track>> GetUserLibraryAsync(Guid userId)
        {
            return await _dc.Tracks.Where(st => st.UserId == userId).ToListAsync();
        }

        public async Task AddTrackAsync(Track track) =>
            await _dc.Tracks.AddAsync(track);

        public void RemoveTrack(Track track) =>
            _dc.Tracks.Remove(track);

        public async Task<bool> ExistsAsync(Guid userId, Guid trackId) =>
            await _dc.Tracks.AnyAsync(st => st.UserId == userId && st.Id == trackId.ToString());
    }
}
