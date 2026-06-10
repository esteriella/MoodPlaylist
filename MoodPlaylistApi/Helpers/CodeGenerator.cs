namespace MoodPlaylistApi.Helpers
{
    public class CodeGenerator
    {   
        public static Task<string> Generate(string initial)
        {
            // Get the last 5 digits of the milliseconds part of the timestamp
            string timestampString = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            string timestampSubstring = timestampString[^5..]; // Last 5 digits of milliseconds

            // Generate part from Guid 
            string ulidPart = Guid.CreateVersion7().ToString()[^5..]; // Get last 5 characters

            // Combine the parts to get the desired length
            string generatedCode = $"{initial}{timestampSubstring}{ulidPart}";
            return Task.FromResult(generatedCode);
        }

    }
}
