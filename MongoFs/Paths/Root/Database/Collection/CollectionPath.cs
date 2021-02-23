using MongoFs.Paths.Abstract;

namespace MongoFs.Paths.Root.Database.Collection
{
    public sealed class CollectionPath : PathWithCollection
    {
        public CollectionPath(string database, string collection) : base(database, collection) {}
    }
}
