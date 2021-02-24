using System.Collections.Generic;
using MongoDB.Bson;

namespace MongoFs
{
    public interface IMongoDb
    {
        BsonContainer<BsonDocument> GetCurrentOp();

        BsonContainer<BsonDocument> GetServerStatus();

        BsonContainer<BsonDocument> GetBuildInfo();

        BsonContainer<BsonDocument> GetHostInfo();

        BsonContainer<BsonDocument> GetListCommands();

        IEnumerable<string> GetDatabases();

        IEnumerable<string> GetCollections(string db);

        long GetTotalDbSize();

        long GetDatabaseSize(string db);

        long GetCollectionSize(string db, string collection);

        long GetDocumentCount(string db, string collection);

        BsonContainer<BsonDocument> GetStats(string db, string collection);

        BsonContainer<BsonDocument> GetIndexes(string db, string collection);

        IEnumerable<BsonContainer<BsonDocument>> GetAllDocuments(string db, string collection);

        BsonContainer<BsonDocument>? GetDocumentAt(string db, string collection, long index);

        BsonContainer<BsonDocument>? QueryDocumentAt(string db, string collection, long index, string field, string query);

        IEnumerable<BsonContainer<BsonDocument>> QueryAllDocuments(string db, string collection, string field, string query);
    }
}
