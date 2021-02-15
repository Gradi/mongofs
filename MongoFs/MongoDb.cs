using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoFs.Extensions;
using MongoFs.Paths;

namespace MongoFs
{
    public class MongoDb : IMongoDb
    {
        private readonly IMongoClient _client;

        public MongoDb(string connectionString)
        {
            _client = new MongoClient(connectionString);
        }

        public IEnumerable<string> GetDatabases() =>
            _client.ListDatabaseNames().ToList();

        public IEnumerable<string> GetCollections(string db) =>
            _client.GetDatabase(db).ListCollectionNames().ToList();

        public long GetTotalDbSize()
        {
            long sum = 0;
            foreach (var db in GetDatabases())
            {
                sum += GetDatabaseSize(db);
            }
            return sum;
        }

        public long GetDatabaseSize(string database)
        {
            var dbStats = _client.GetDatabase(database).RunCommand<BsonDocument>(new BsonDocument
            {
                { "dbStats", 1 }
            });

            if (dbStats.TryGetValue("totalSize", out var size) || dbStats.TryGetValue("dataSize", out size))
            {
                return (long)size.AsAnyNumber();
            }
            return 0;
        }

        public long GetCollectionSize(string db, string collection)
        {
            var collStats = GetStats(db, collection).BsonDocument;
            if (collStats.TryGetValue("size", out var size))
            {
                return (long)size.AsAnyNumber();
            }
            return 0;
        }

        public BsonDocumentContainer GetStats(string db, string collection)
        {
            return new BsonDocumentContainer(_client.GetDatabase(db).RunCommand<BsonDocument>(new BsonDocument
            {
                { "collStats", collection }
            }));
        }

        public BsonDocumentContainer GetIndexes(string db, string collection)
        {
            var indexes = _client.GetDatabase(db).GetCollection<BsonDocument>(collection)
            .Indexes.List().ToList();

            var document = new BsonDocument();
            document.Add(new BsonElement("Indexes", new BsonArray(indexes)));
            return new BsonDocumentContainer(document);
        }
    }
}
