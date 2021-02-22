using System;
using System.IO;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoFs.Paths
{
    public class BsonDocumentContainer
    {
        private static readonly JsonWriterSettings JsonWriterSettings;

        public BsonDocument BsonDocument { get; }

        static BsonDocumentContainer()
        {
            JsonWriterSettings = JsonWriterSettings.Defaults.Clone();
            JsonWriterSettings.Indent = true;
            JsonWriterSettings.IndentChars = "    ";
        }

        public BsonDocumentContainer(BsonDocument document)
        {
            BsonDocument = document ?? throw new ArgumentNullException(nameof(document));
        }

        public string AsJsonString() => BsonDocument.ToJson(JsonWriterSettings);

        public byte[] AsJsonBytes() => Encoding.UTF8.GetBytes(AsJsonString());

        public long GetJsonBytesLength() => Encoding.UTF8.GetByteCount(AsJsonString());

        public byte[] AsBsonBytes()
        {
            using var stream = new MemoryStream();
            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, BsonDocument);
            }
            return stream.ToArray();
        }

        public long GetBsonBytesLength() => AsBsonBytes().Length;
    }
}
