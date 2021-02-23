using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace MongoFs.Extensions
{
    public static class BsonValueExtensions
    {
        private static readonly JsonWriterSettings JsonWriterSettings;

        static BsonValueExtensions()
        {
            JsonWriterSettings = JsonWriterSettings.Defaults.Clone();
            JsonWriterSettings.Indent = true;
            JsonWriterSettings.IndentChars = "    ";
        }

        public static double AsAnyNumber(this BsonValue value) => value.BsonType switch
        {
            BsonType.Int32 => (double)value.AsInt32,
            BsonType.Int64 => (double)value.AsInt64,
            BsonType.Double => value.AsDouble,
            BsonType.Decimal128 => (double)value.AsDecimal,
            var t => throw new ArgumentException($"Can't interpret bson type {t} as number.")
        };

        public static string ToIndentedJson(this BsonValue value) => value.ToJson(JsonWriterSettings);
    }
}
