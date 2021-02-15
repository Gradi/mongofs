namespace MongoFs.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidDbOrCollName(this string? str) =>
            !string.IsNullOrEmpty(str) && !str.Contains('.');
    }
}
