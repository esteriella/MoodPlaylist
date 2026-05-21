namespace MoodPlaylistApi.Interfaces
{
    public interface IUnitOfWork
    {
        public IAuthRepository AuthRepository { get; }
        IMoodRepository Moods { get; }
        ITrackRepository Tracks { get; }
        ILibraryRepository Library { get; }
    }
}
