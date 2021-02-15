using System;
using System.IO;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MongoFs.Paths
{
    public class BsonDocumentContainer
    {
        public BsonDocument BsonDocument { get; }

        public BsonDocumentContainer(BsonDocument document)
        {
            BsonDocument = document ?? throw new ArgumentNullException(nameof(document));
        }

        public string AsJsonString() => JObject.Parse(BsonDocument.ToJson()).ToString(Formatting.Indented);

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
