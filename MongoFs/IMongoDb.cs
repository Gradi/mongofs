using System.Collections.Generic;
using MongoFs.Paths;

namespace MongoFs
{
    public interface IMongoDb
    {
        IEnumerable<string> GetDatabases();

        IEnumerable<string> GetCollections(string db);

        long GetTotalDbSize();

        long GetDatabaseSize(string database);

        long GetCollectionSize(string db, string collection);

        BsonDocumentContainer GetStats(string db, string collection);

        BsonDocumentContainer GetIndexes(string db, string collection);
    }
}
