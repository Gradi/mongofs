using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoFs.Extensions
{
    public static class FindFluentExtensions
    {
        public static IFindFluent<BsonDocument, BsonDocument> SortById(this IFindFluent<BsonDocument, BsonDocument> cursor) =>
            cursor.Sort(Builders<BsonDocument>.Sort.Ascending("_id"));
    }
}
