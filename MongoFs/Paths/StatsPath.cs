namespace MongoFs.Paths
{
    public sealed class StatsPath : PathWithCollection
    {
        public const string FileName = "stats.json";

        public StatsPath(string database, string collection) : base(database, collection) {}
    }
}
