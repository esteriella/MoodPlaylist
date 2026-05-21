using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Interfaces;

namespace MoodPlaylistApi.Data.Repositories
{
    public sealed class TrackRepository(AppDbContext dc) : ITrackRepository
    {
        private readonly AppDbContext _dc = dc;

        public async Task<IEnumerable<Track>> GetAllAsync() =>
            await _dc.Tracks.ToListAsync();

        public async Task<Track?> GetByIdAsync(Guid id) =>
            await _dc.Tracks.FindAsync(id);

        public async Task AddAsync(Track track) =>
            await _dc.Tracks.AddAsync(track);

        public void Update(Track track) =>
            _dc.Tracks.Update(track);

        public void Remove(Track track) =>
            _dc.Tracks.Remove(track);

        public async Task<IEnumerable<Track>> GetByPlaylistIdAsync(Guid playlistId) =>
            await _dc.Tracks
                     .Where(t => EF.Property<Guid>(t, "PlaylistId") == playlistId)
                     .ToListAsync();

        public async Task<IEnumerable<Track>> GetByMoodIdAsync(Guid moodId) =>
            await _dc.Tracks
                     .Where(t => EF.Property<Guid>(t, "MoodId") == moodId)
                     .ToListAsync();
    }

}
