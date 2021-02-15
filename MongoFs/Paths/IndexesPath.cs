namespace MongoFs.Paths
{
    public sealed class IndexesPath : PathWithCollection
    {
        public const string FileName = "indexes.json";

        public IndexesPath(string database, string collection) : base(database, collection) {}
    }
}
