using System;

namespace MongoFs.Paths
{
    public abstract class PathWithDatabase : Path
    {
        public string Database { get; }

        protected PathWithDatabase(string database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }
    }
}
