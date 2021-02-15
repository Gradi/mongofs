namespace MongoFs.Paths
{
    public sealed class DatabasePath : PathWithDatabase
    {
        public DatabasePath(string database) : base(database) {}
    }
}
