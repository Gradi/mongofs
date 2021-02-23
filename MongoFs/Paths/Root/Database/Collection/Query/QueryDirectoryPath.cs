using MongoFs.Paths.Abstract;

namespace MongoFs.Paths.Root.Database.Collection.Query
{
    public sealed class QueryDirectoryPath : PathWithQuery
    {
        public const string FileName = "query";

        public QueryDirectoryPath(string database, string collection, string query, string field)
            : base(database, collection, query, field) {}
    }
}
