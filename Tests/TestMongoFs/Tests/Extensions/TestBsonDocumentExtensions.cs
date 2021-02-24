using MongoDB.Bson;
using MongoFs.Extensions;
using NUnit.Framework;

namespace TestMongoFs.Tests.Extensions
{
    [TestFixture]
    public class TestBsonDocumentExtensions
    {
        [Test]
        public void TryGetCreationTimeFromId()
        {
            var objId = ObjectId.GenerateNewId();
            var doc = new BsonDocument();
            doc.Add("_id", new BsonObjectId(objId));

            Assert.That(doc.TryGetCreationTimeFromId(), Is.EqualTo(objId.CreationTime.ToLocalTime()));
        }

    }
}
