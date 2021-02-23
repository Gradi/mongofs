using MongoFs.Paths.Abstract;

namespace MongoFs.Paths.Root.Database.Collection.Query
{
    public sealed class QueryAllDocumentsPath : PathWithQuery
    {
        public const string FileName = "all.json";

        public QueryAllDocumentsPath(string database, string collection, string query, string field)
            : base(database, collection, query, field)
        {
        }
    }
}
