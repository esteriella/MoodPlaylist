namespace MoodPlaylistApi.Helpers
{
    public class CodeGenerator
    {   
        public static Task<string> Generate()
        {
            // Get the last 5 digits of the milliseconds part of the timestamp
            string timestampString = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            string timestampSubstring = timestampString[^5..]; // Last 5 digits of milliseconds

            // Generate a numeric part from Guid 
            int ulidHasHCode = Math.Abs(Guid.CreateVersion7().GetHashCode()); // Absolute value to ensure positivity
            string ulidPart = ulidHasHCode.ToString().PadLeft(5, '0')[..5]; // Get last 5 digits

            // Combine the parts to get the desired length
            string generatedCode = $"{timestampSubstring}{ulidPart}";
            return Task.FromResult(generatedCode);
        }

        public static Task<string> Generate(string initial)
        {
            // Get the last 5 digits of the milliseconds part of the timestamp
            string timestampString = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            string timestampSubstring = timestampString[^5..]; // Last 5 digits of milliseconds

            // Generate a numeric part from Guid 
            int ulidHasHCode = Math.Abs(Guid.CreateVersion7().GetHashCode()); // Absolute value to ensure positivity
            string ulidPart = ulidHasHCode.ToString().PadLeft(5, '0')[..5]; // Get last 5 digits

            // Combine the parts to get the desired length
            string generatedCode = $"{initial}{timestampSubstring}{ulidPart}";
            return Task.FromResult(generatedCode);
        }

    }
}
