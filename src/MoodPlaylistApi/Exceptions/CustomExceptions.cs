namespace MoodPlaylistApi.Exceptions
{
    public abstract class MoodPlaylistException(string message, Exception? inner = null)
        : Exception(message, inner);

    // Specific exception types
    public class MoodNotFoundException(Guid moodId) : MoodPlaylistException($"Mood with ID {moodId} was not found.")
    {
    }

    // Specific exception types
    public class MoodGenreNotValidException(string message) : MoodPlaylistException(message)
    {
    }

    public class TrackNotFoundException(string trackId) : MoodPlaylistException($"Track with ID {trackId} was not found.")
    {
    }

    public class SpotifyApiException(string message, Exception? inner = null) : MoodPlaylistException(message, inner)
    {
    }

    public class RecommendationRequestException(string message) : MoodPlaylistException(message)
    {
    }

    public class PlaylistCreationException(string message) : MoodPlaylistException(message)
    {
    }
}
