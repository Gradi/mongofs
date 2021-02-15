using System;

namespace MongoFs.Paths
{
    public abstract class PathWithCollection : PathWithDatabase
    {
        public string Collection { get; }

        protected PathWithCollection(string database, string collection) : base(database)
        {
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }
    }
}
