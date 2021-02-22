using System.Collections.Generic;
using MongoFs.Paths;

namespace MongoFs
{
    public interface IMongoDb
    {
        IEnumerable<string> GetDatabases();

        IEnumerable<string> GetCollections(string db);

        long GetTotalDbSize();

        long GetDatabaseSize(string db);

        long GetCollectionSize(string db, string collection);

        long GetDocumentCount(string db, string collection);

        BsonDocumentContainer GetStats(string db, string collection);

        BsonDocumentContainer GetIndexes(string db, string collection);

        IEnumerable<BsonDocumentContainer> GetAllDocuments(string db, string collection);

        BsonDocumentContainer? GetDocumentAt(string db, string collection, long index);
    }
}
