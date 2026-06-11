namespace MoodPlaylistApi.Interfaces
{
    public interface IUnitOfWork
    {
        public IAuthRepository AuthRepository { get; }
        public ILibraryRepository LibraryRepository { get; }
    }
}
