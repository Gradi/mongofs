namespace MongoFs.Paths
{
    public interface IPathParser
    {
        Path? Parse(string? pathStr);
    }
}
