using MoodPlaylistApi.Data.Repositories;
using MoodPlaylistApi.Interfaces;

namespace MoodPlaylistApi.Data
{
    public class UnitOfWork (AppDbContext dc) : IUnitOfWork 
    {
        public IAuthRepository AuthRepository => new AuthRepository(dc);
        public IMoodRepository MoodRepository => new MoodRepository(dc);
        public ITrackRepository TrackRepository => new TrackRepository(dc)

    }
}
