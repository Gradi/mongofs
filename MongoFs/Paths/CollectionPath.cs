namespace MongoFs.Paths
{
    public sealed class CollectionPath : PathWithCollection
    {
        public CollectionPath(string database, string collection) : base(database, collection) {}
    }
}
