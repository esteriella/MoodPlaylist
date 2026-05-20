namespace MoodPlaylistApi.Interfaces
{
    public interface IUnitOfWork
    {
        public IAuthRepository AuthRepository { get; }
    }
}
