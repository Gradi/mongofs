using MongoFs.Paths.Abstract;

namespace MongoFs.Paths.Root.Database.Collection.Data
{
    public sealed class DataDirectoryPath : PathWithCollection
    {
        public const string FileName = "data";

        public DataDirectoryPath(string database, string collection) : base(database, collection) {}
    }
}
