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
        public Path? InvalidInputs(string? input) => _parser.Parse(input);

        [Test]
        public void TestRootPath() => Assert.That(_parser.Parse("/"), Is.TypeOf<RootPath>());

        [Test]
        public void TestDatabasePath()
        {
            var name = TestContext.CurrentContext.Random.GetString();

            void Check(DatabasePath db) => Assert.That(db.Database, Is.EqualTo(name));
            AssertResult<DatabasePath>($"/{name}", Check);
            AssertResult<DatabasePath>($"/{name}/", Check);
        }

        [Test]
        public void TestCollectionPath()
        {
            var db = TestContext.CurrentContext.Random.GetString();
            var collection = TestContext.CurrentContext.Random.GetString();

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
        public void TestSimple3SegmentsPaths(string lastSegment, Type expectedType)
        {
            var db = TestContext.CurrentContext.Random.GetString();
            var collection = TestContext.CurrentContext.Random.GetString();

            void Check(object? result)
            {
                Assert.That(result, Is.TypeOf(expectedType));
                Assert.That(((PathWithCollection)result!).Database, Is.EqualTo(db));
                Assert.That(((PathWithCollection)result).Collection, Is.EqualTo(collection));
            }

            Check(_parser.Parse($"/{db}/{collection}/{lastSegment}"));
            Check(_parser.Parse($"/{db}/{collection}/{lastSegment}/"));
        }

        private void AssertResult<T>(string? input, Action<T> action) where T : Path
        {
            var result = _parser.Parse(input);
            Assert.That(result, Is.TypeOf<T>());
            action((T)result!);
        }
    }
}
