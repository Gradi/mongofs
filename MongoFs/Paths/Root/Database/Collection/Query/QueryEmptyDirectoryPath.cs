using MongoFs.Paths.Abstract;

namespace MongoFs.Paths.Root.Database.Collection.Query
{
    public sealed class QueryEmptyDirectoryPath : PathWithCollection
    {
        public QueryEmptyDirectoryPath(string database, string collection) : base(database, collection) {}
    }
}
