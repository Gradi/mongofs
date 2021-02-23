using System;

namespace MongoFs.Paths.Abstract
{
    public abstract class PathWithQuery : PathWithCollection
    {
        public string Query { get; }

        public string Field { get; }

        protected PathWithQuery(string database, string collection, string query, string field) : base(database, collection)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
            Field = field ?? throw new ArgumentNullException(nameof(field));
        }
    }
}
