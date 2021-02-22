using System;
using System.Text.RegularExpressions;
using MongoFs.Paths;

namespace MongoFs.Extensions
{
    public static class PathParserStringExtensions
    {
        // Examples: 0.json, 123.bson, 55.json
        private static readonly Regex DocumentIndex = new (@"^([0-9]+)\.((?:json|bson))$", RegexOptions.Compiled);

        public static bool IsValidDbOrCollName(this string? str) =>
            !string.IsNullOrEmpty(str) && !str.Contains('.');

        public static bool IsValidDocumentIndex(this string? str, out long index, out DataDocumentType type)
        {
            index = 0;
            type = default;

            if (str == null) return false;

            var match = DocumentIndex.Match(str);
            if (!match.Success) return false;

            index = long.Parse(match.Groups[1].Value);
            type = match.Groups[2].Value switch
            {
                "json" => DataDocumentType.Json,
                "bson" => DataDocumentType.Bson,
                var val => throw new ArgumentException($"Unrecognized document extension ({val}).")
            };
            return true;
        }
    }
}
