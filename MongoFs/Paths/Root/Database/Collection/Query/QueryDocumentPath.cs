using MongoFs.Paths.Abstract;

namespace MongoFs.Paths.Root.Database.Collection.Query
{
    public sealed class QueryDocumentPath : PathWithQuery
    {
        public long Index { get; }

        public DataDocumentType Type { get; }

        public string FileName => $"{Index}.{Type.ToString().ToLower()}";

        public QueryDocumentPath(string database, string collection, string query, string field, long index,
                                 DataDocumentType type)
            : base(database, collection, query, field)
        {
            Index = index;
            Type = type;
        }
    }
}
