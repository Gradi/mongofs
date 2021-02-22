using System;
using DokanNet;
using MongoFs.Paths;
using Serilog;

namespace MongoFs.Handlers
{
    public class ReadFileHandler : BaseHandler
    {
        public ReadFileHandler
            (
                ILogger logger,
                IMongoDb mongoDb
            ) : base(logger, mongoDb)
        {
        }

        public NtStatus ReadFile(Path path, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            bytesRead = 0;
            return path switch
            {
                IndexesPath p      => ReadFile(p, buffer, out bytesRead, offset),
                StatsPath p        => ReadFile(p, buffer, out bytesRead, offset),
                DataDocumentPath p => ReadFile(p, buffer, out bytesRead, offset),
                var p              => LogFailure(p)
            };
        }

        private NtStatus ReadFile(IndexesPath path, byte[] buffer, out int bytesRead, long offset) =>
            ReadBuffer(_mongoDb.GetIndexes(path.Database, path.Collection).AsJsonBytes(), buffer, out bytesRead, offset);

        private NtStatus ReadFile(StatsPath path, byte[] buffer, out int bytesRead, long offset) =>
            ReadBuffer(_mongoDb.GetStats(path.Database, path.Collection).AsJsonBytes(), buffer, out bytesRead, offset);

        private NtStatus ReadFile(DataDocumentPath path, byte[] buffer, out int bytesRead, long offset)
        {
            var document = _mongoDb.GetDocumentAt(path.Database, path.Collection, path.Index);
            if (document == null)
            {
                bytesRead = 0;
                return NtStatus.NoSuchFile;
            }

            var sourceBytes = path.Type switch
            {
                DataDocumentType.Json => document.AsJsonBytes(),
                DataDocumentType.Bson => document.AsBsonBytes(),
                _ => throw new ArgumentOutOfRangeException($"Unsupported path type {path.Type}.")
            };
            return ReadBuffer(sourceBytes, buffer, out bytesRead, offset);
        }

        private NtStatus ReadBuffer(byte[] source, byte[] destination, out int bytesRead, long offset)
        {
            if (offset >= source.Length)
            {
                _logger.Debug("Request to read bytes at {Offset} offset, but source array's length is less than offset ({SourceLength}).",
                    offset, source.Length);
                bytesRead = 0;
                return NtStatus.Error;
            }

            bytesRead = Math.Min(source.Length - (int)offset, destination.Length);
            Array.Copy(source, offset, destination, 0, bytesRead);
            return NtStatus.Success;
        }
    }
}
