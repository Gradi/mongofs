using MongoFs.Paths.Abstract;

namespace MongoFs.Paths.Root.Database.Collection
{
    public sealed class IndexesPath : PathWithCollection
    {
        public const string FileName = "indexes.json";

        public IndexesPath(string database, string collection) : base(database, collection) {}
    }
}
