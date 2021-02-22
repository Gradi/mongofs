using System;
using MongoFs.Paths;
using NUnit.Framework;
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

            void Check(Path? path, DataDocumentType expectedType)
            {
                Assert.That(path, Is.TypeOf<DataDocumentPath>());
                var doc = (DataDocumentPath)path!;
                Assert.That(doc.Database, Is.EqualTo(db));
                Assert.That(doc.Collection, Is.EqualTo(coll));
                Assert.That(doc.Index, Is.EqualTo(index));
                Assert.That(doc.Type, Is.EqualTo(expectedType));
            }
            Check(_parser.Parse($"/{db}/{coll}/data/{index}.json"), DataDocumentType.Json);
            Check(_parser.Parse($"/{db}/{coll}/data/{index}.bson"), DataDocumentType.Bson);
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
