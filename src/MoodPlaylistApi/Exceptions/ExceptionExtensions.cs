namespace MoodPlaylistApi.Exceptions
{
    public static class ExceptionExtensions
    {
        // Convert exception to a detailed string
        public static string ToDetailedString(this Exception ex)
        {
            return $"Exception: {ex.Message}\n" +
                   $"Type: {ex.GetType().Name}\n" +
                   $"StackTrace: {ex.StackTrace}";
        }

        // Extract inner exception messages recursively
        public static string GetAllMessages(this Exception ex)
        {
            var messages = new List<string>();
            while (ex is not null)
            {
                messages.Add(ex.Message);
            }
            return string.Join(" --> ", messages);
        }
    }
}
