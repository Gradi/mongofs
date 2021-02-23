using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoFs.Extensions;

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
            var collStats = GetStats(db, collection).Value;
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

        public BsonContainer<BsonDocument> GetStats(string db, string collection)
        {
            return new BsonContainer<BsonDocument>(_client.GetDatabase(db).RunCommand<BsonDocument>(new BsonDocument
            {
                { "collStats", collection }
            }));
        }

        public BsonContainer<BsonDocument> GetIndexes(string db, string collection)
        {
            var indexes = _client.GetDatabase(db).GetCollection<BsonDocument>(collection)
            .Indexes.List().ToList();

            var document = new BsonDocument();
            document.Add(new BsonElement("Indexes", new BsonArray(indexes)));
            return new BsonContainer<BsonDocument>(document);
        }

        public IEnumerable<BsonContainer<BsonDocument>> GetAllDocuments(string db, string collection) =>
            QueryAllDocuments(db, collection, Builders<BsonDocument>.Filter.Empty);

        public BsonContainer<BsonDocument>? GetDocumentAt(string db, string collection, long index) =>
            QueryDocumentAt(db, collection, Builders<BsonDocument>.Filter.Empty, index);

        public BsonContainer<BsonDocument>? QueryDocumentAt(string db, string collection, long index, string field, string query) =>
            QueryDocumentAt(db, collection, QueryToFilter(field, query), index);

        public IEnumerable<BsonContainer<BsonDocument>> QueryAllDocuments(string db, string collection, string field, string query) =>
            QueryAllDocuments(db, collection, QueryToFilter(field, query));

        private BsonContainer<BsonDocument>? QueryDocumentAt(string db, string collection, FilterDefinition<BsonDocument> filter, long index)
        {
            var document = _client
                .GetDatabase(db)
                .GetCollection<BsonDocument>(collection)
                .Find(filter)
                .SortById()
                .Skip((int)index)
                .Limit(1)
                .SingleOrDefault();

            return document != null ? new BsonContainer<BsonDocument>(document) : null;
        }

        private IEnumerable<BsonContainer<BsonDocument>> QueryAllDocuments(string db, string collection, FilterDefinition<BsonDocument> filter)
        {
            return _client
                .GetDatabase(db)
                .GetCollection<BsonDocument>(collection)
                .Find(filter)
                .SortById()
                .ToEnumerable()
                .Select(d => new BsonContainer<BsonDocument>(d));
        }

        private FilterDefinition<BsonDocument> QueryToFilter(string field, string query)
        {
            if (string.IsNullOrEmpty(field)) throw new ArgumentNullException(nameof(field));
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var values = ToPossibleBsonValues(query);
            var filters = values.Select(v => Builders<BsonDocument>.Filter.Eq(field, v));
            return Builders<BsonDocument>.Filter.Or(filters);
        }

        private IReadOnlyCollection<BsonValue> ToPossibleBsonValues(string query)
        {
            var result = new List<BsonValue>(8);

            if (double.TryParse(query, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var dresult))
            {
                result.Add(new BsonDouble(dresult));
            }

            result.Add(new BsonString(query));

            if (ObjectId.TryParse(query, out var objIdResult))
                result.Add(new BsonObjectId(objIdResult));

            if (query == "true")
                result.Add(new BsonBoolean(true));
            else if (query == "false")
                result.Add(new BsonBoolean(false));

            if (DateTime.TryParse(query, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtResult))
                result.Add(new BsonDateTime(dtResult));

            if (query == "null")
                result.Add(BsonNull.Value);

            if (int.TryParse(query, out var iresult))
                result.Add(new BsonInt32(iresult));

            if (long.TryParse(query, out var lresult))
                result.Add(new BsonInt64(lresult));

            return result;
        }
    }
}
