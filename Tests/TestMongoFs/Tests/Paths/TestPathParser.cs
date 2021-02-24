using MongoFs.Paths.Abstract;
using MongoFs.Paths.Root.Database.Collection.Data;
using MongoFs.Paths.Root.Database.Collection.Query;
using MongoFs.Paths.Root.Database.Collection;
using MongoFs.Paths.Root.Database;
using MongoFs.Paths.Root;
using MongoFs.Paths;
using NUnit.Framework;
using System;
using TestMongoFs.Mocks;

namespace TestMongoFs.Tests.Paths
{
    [TestFixture]
    public class TestPathParser
    {
        private readonly PathParser _parser;

        public TestPathParser()
        {
            _parser = new PathParser(new NullLogger(), '/');
        }

        [TestCase(null, ExpectedResult = null)]
        [TestCase("", ExpectedResult = null)]
        [TestCase("something", ExpectedResult = null)]
        public Path? InvalidInputs(string? input) => _parser.Parse(input);

        [Test]
        public void TestRootPath() => Assert.That(_parser.Parse("/"), Is.TypeOf<RootPath>());

        [Test]
        public void TestDatabasePath()
        {
            var (database, _) = GenRandomDbColl();

            void Check(DatabasePath db) => Assert.That(db.Database, Is.EqualTo(database));
            AssertResult<DatabasePath>($"/{database}", Check);
            AssertResult<DatabasePath>($"/{database}/", Check);
        }

        [Test]
        public void Test0SegmentsPaths()
        {
            AssertResult<CurrentOpPath>("/currentOp.json", _ => {});
            AssertResult<ServerStatusPath>("/serverStatus.json", _ => {});
            AssertResult<BuildInfoPath>("/buildInfo.json", _ => {});
            AssertResult<HostInfoPath>("/hostInfo.json", _ => {});
            AssertResult<ListCommandsPath>("/listCommands.json", _ => {});
        }

        [Test]
        public void TestCollectionPath()
        {
            var (db, collection) = GenRandomDbColl();

            void Check(CollectionPath coll)
            {
                Assert.That(coll.Database, Is.EqualTo(db));
                Assert.That(coll.Collection, Is.EqualTo(collection));
            }

            AssertResult<CollectionPath>($"/{db}/{collection}", Check);
            AssertResult<CollectionPath>($"/{db}/{collection}/", Check);
        }

        [TestCase("stats.json", typeof(StatsPath))]
        [TestCase("indexes.json", typeof(IndexesPath))]
        [TestCase("data", typeof(DataDirectoryPath))]
        [TestCase("query", typeof(QueryEmptyDirectoryPath))]
        public void TestSimple3SegmentsPaths(string lastSegment, Type expectedType)
        {
            var (db, collection) = GenRandomDbColl();

            void Check(object? result)
            {
                Assert.That(result, Is.TypeOf(expectedType));
                Assert.That(((PathWithCollection)result!).Database, Is.EqualTo(db));
                Assert.That(((PathWithCollection)result).Collection, Is.EqualTo(collection));
            }

            Check(_parser.Parse($"/{db}/{collection}/{lastSegment}"));
            Check(_parser.Parse($"/{db}/{collection}/{lastSegment}/"));
        }

        [Test]
        public void TestDataDocumentPath()
        {
            var (db, coll) = GenRandomDbColl();
            long index = TestContext.CurrentContext.Random.NextLong(1000);

            void Check(DataDocumentPath path, DataDocumentType expectedType)
            {
                Assert.That(path.Database, Is.EqualTo(db));
                Assert.That(path.Collection, Is.EqualTo(coll));
                Assert.That(path.Index, Is.EqualTo(index));
                Assert.That(path.Type, Is.EqualTo(expectedType));
            }
            AssertResult<DataDocumentPath>($"/{db}/{coll}/data/{index}.json", p => Check(p, DataDocumentType.Json));
            AssertResult<DataDocumentPath>($"/{db}/{coll}/data/{index}.bson", p => Check(p, DataDocumentType.Bson));
        }

        [Test]
        public void TestQueryDirectoryPath()
        {
            var (database, collection) = GenRandomDbColl();
            var query = TestContext.CurrentContext.Random.GetString();
            var field = TestContext.CurrentContext.Random.GetString();

            void Check(QueryDirectoryPath path, string expectedField)
            {
                Assert.That(path.Database, Is.EqualTo(database));
                Assert.That(path.Collection, Is.EqualTo(collection));
                Assert.That(path.Query, Is.EqualTo(query));
                Assert.That(path.Field, Is.EqualTo(expectedField));
            }

            AssertResult<QueryDirectoryPath>($"/{database}/{collection}/query/{query}", p => Check(p, "_id"));
            AssertResult<QueryDirectoryPath>($"/{database}/{collection}/query/{field}/{query}", p => Check(p, field));
        }

        [Test]
        public void TestQueryDocumentPath()
        {
            var (database, collection) = GenRandomDbColl();
            var query = TestContext.CurrentContext.Random.GetString();
            var field = TestContext.CurrentContext.Random.GetString();
            var index = TestContext.CurrentContext.Random.NextLong(1000);

            void Check(QueryDocumentPath path, string expectedField, DataDocumentType expectedType)
            {
                Assert.That(path.Database, Is.EqualTo(database));
                Assert.That(path.Collection, Is.EqualTo(collection));
                Assert.That(path.Query, Is.EqualTo(query));
                Assert.That(path.Field, Is.EqualTo(expectedField));
                Assert.That(path.Index, Is.EqualTo(index));
                Assert.That(path.Type, Is.EqualTo(expectedType));
            }

            AssertResult<QueryDocumentPath>($"/{database}/{collection}/query/{query}/{index}.json",
                p => Check(p, "_id", DataDocumentType.Json));
            AssertResult<QueryDocumentPath>($"/{database}/{collection}/query/{query}/{index}.bson",
                p => Check(p, "_id", DataDocumentType.Bson));

            AssertResult<QueryDocumentPath>($"/{database}/{collection}/query/{field}/{query}/{index}.json",
                p => Check(p, field, DataDocumentType.Json));
            AssertResult<QueryDocumentPath>($"/{database}/{collection}/query/{field}/{query}/{index}.bson",
                p => Check(p, field, DataDocumentType.Bson));
        }

        [Test]
        public void TestQueryAllDocumentsPath()
        {
            var (database, collection) = GenRandomDbColl();
            var query = TestContext.CurrentContext.Random.GetString();
            var field = TestContext.CurrentContext.Random.GetString();

            void Check(QueryAllDocumentsPath p, string expectedField)
            {
                Assert.That(p.Database, Is.EqualTo(database));
                Assert.That(p.Collection, Is.EqualTo(collection));
                Assert.That(p.Query, Is.EqualTo(query));
                Assert.That(p.Field, Is.EqualTo(expectedField));
            }

            AssertResult<QueryAllDocumentsPath>($"/{database}/{collection}/query/{query}/all.json",
                p => Check(p, "_id"));
            AssertResult<QueryAllDocumentsPath>($"/{database}/{collection}/query/{field}/{query}/all.json",
                p => Check(p, field));
        }

        private void AssertResult<T>(string? input, Action<T> action) where T : Path
        {
            var result = _parser.Parse(input);
            Assert.That(result, Is.TypeOf<T>());
            action((T)result!);
        }

        private (string Database, string Collection) GenRandomDbColl() =>
            (TestContext.CurrentContext.Random.GetString(), TestContext.CurrentContext.Random.GetString());
    }
}
