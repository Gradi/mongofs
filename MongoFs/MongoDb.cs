using System.Collections.Generic;
using System.Linq;
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

        public long GetDatabaseSize(string db)
        {
            var dbStats = _client.GetDatabase(db).RunCommand<BsonDocument>(new BsonDocument
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

        public long GetDocumentCount(string db, string collection)
        {
            return _client
                .GetDatabase(db)
                .GetCollection<BsonDocument>(collection)
                .CountDocuments(Builders<BsonDocument>.Filter.Empty);
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

        public IEnumerable<BsonDocumentContainer> GetAllDocuments(string db, string collection)
        {
            return _client
                .GetDatabase(db)
                .GetCollection<BsonDocument>(collection)
                .Find(Builders<BsonDocument>.Filter.Empty)
                .SortById()
                .ToEnumerable()
                .Select(d => new BsonDocumentContainer(d));
        }

        public BsonDocumentContainer? GetDocumentAt(string db, string collection, long index)
        {
            var document = _client
                .GetDatabase(db)
                .GetCollection<BsonDocument>(collection)
                .Find(Builders<BsonDocument>.Filter.Empty)
                .SortById()
                .Skip((int)index)
                .Limit(1)
                .SingleOrDefault();

            return document != null ? new BsonDocumentContainer(document) : null;
        }
    }
}
