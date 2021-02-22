namespace MongoFs.Paths
{
    public sealed class DataDocumentPath : PathWithCollection
    {
        public long Index { get; }

        public DataDocumentType Type { get; }

        public string FileName => $"{Index}.{Type.ToString().ToLower()}";

        public DataDocumentPath(string database, string collection, long index, DataDocumentType type) : base(database, collection)
        {
            Index = index;
            Type = type;
        }
    }
}
