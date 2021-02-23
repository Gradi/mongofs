using System;
using System.IO;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoFs.Extensions;

namespace MongoFs
{
    public class BsonContainer<T> where T : BsonValue
    {
        public T Value { get; }

        public BsonContainer(T value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string AsJsonString() => Value.ToIndentedJson();

        public byte[] AsJsonBytes() => Encoding.UTF8.GetBytes(AsJsonString());

        public long GetJsonBytesLength() => Encoding.UTF8.GetByteCount(AsJsonString());

        public byte[] AsBsonBytes()
        {
            using var stream = new MemoryStream();
            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, Value);
            }
            return stream.ToArray();
        }

        public long GetBsonBytesLength() => AsBsonBytes().Length;
    }
}
