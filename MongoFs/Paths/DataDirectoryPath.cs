namespace MongoFs.Paths
{
    public sealed class DataDirectoryPath : PathWithCollection
    {
        public const string FileName = "data";

        public DataDirectoryPath(string database, string collection) : base(database, collection) {}
    }
}
