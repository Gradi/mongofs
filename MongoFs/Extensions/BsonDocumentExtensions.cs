using System;
using MongoDB.Bson;

namespace MongoFs.Extensions
{
    public static class BsonDocumentExtensions
    {
        public static DateTime? TryGetCreationTimeFromId(this BsonDocument document)
        {
            if (document.TryGetElement("_id", out var id) && id.Value.BsonType == BsonType.ObjectId)
            {
                return id.Value.AsObjectId.CreationTime.ToLocalTime();
            }
            return null;
        }
    }
}
