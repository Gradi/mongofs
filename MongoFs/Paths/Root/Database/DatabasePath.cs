using MongoFs.Paths.Abstract;

namespace MongoFs.Paths.Root.Database
{
    public sealed class DatabasePath : PathWithDatabase
    {
        public DatabasePath(string database) : base(database) {}
    }
}
