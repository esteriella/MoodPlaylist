using MoodPlaylistApi.Data.Repositories;
using MoodPlaylistApi.Interfaces;

namespace MoodPlaylistApi.Data
{
    public class UnitOfWork (AppDbContext dc) : IUnitOfWork 
    {
        public IAuthRepository AuthRepository => new AuthRepository(dc);
    }
}
