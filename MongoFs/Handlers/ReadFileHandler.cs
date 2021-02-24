using DokanNet;
using MongoDB.Bson;
using MongoFs.Paths.Abstract;
using MongoFs.Paths.Root.Database.Collection.Data;
using MongoFs.Paths.Root.Database.Collection.Query;
using MongoFs.Paths.Root.Database.Collection;
using MongoFs.Paths;
using Serilog;
using System.Linq;
using System;
using MongoFs.Paths.Root;

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
                IndexesPath p           => ReadFile(p, buffer, out bytesRead, offset),
                StatsPath p             => ReadFile(p, buffer, out bytesRead, offset),
                CurrentOpPath p         => ReadFile(p, buffer, out bytesRead, offset),
                ServerStatusPath p      => ReadFile(p, buffer, out bytesRead, offset),
                BuildInfoPath p         => ReadFile(p, buffer, out bytesRead, offset),
                HostInfoPath p          => ReadFile(p, buffer, out bytesRead, offset),
                ListCommandsPath p      => ReadFile(p, buffer, out bytesRead, offset),
                DataDocumentPath p      => ReadFile(p, buffer, out bytesRead, offset),
                QueryDocumentPath p     => ReadFile(p, buffer, out bytesRead, offset),
                QueryAllDocumentsPath p => ReadFile(p, buffer, out bytesRead, offset),
                QueryDirectoryPath p    => ReadFile(p, buffer, out bytesRead, offset),
                var p                   => LogFailure(p)
            };
        }

        private NtStatus ReadFile(IndexesPath path, byte[] buffer, out int bytesRead, long offset) =>
            ReadBuffer(_mongoDb.GetIndexes(path.Database, path.Collection).AsJsonBytes(), buffer, out bytesRead, offset);

        private NtStatus ReadFile(StatsPath path, byte[] buffer, out int bytesRead, long offset) =>
            ReadBuffer(_mongoDb.GetStats(path.Database, path.Collection).AsJsonBytes(), buffer, out bytesRead, offset);

        private NtStatus ReadFile(CurrentOpPath path, byte[] buffer, out int bytesRead, long offset) =>
            ReadBuffer(_mongoDb.GetCurrentOp().AsJsonBytes(), buffer, out bytesRead, offset);

        private NtStatus ReadFile(ServerStatusPath path, byte[] buffer, out int bytesRead, long offset) =>
            ReadBuffer(_mongoDb.GetServerStatus().AsJsonBytes(), buffer, out bytesRead, offset);

        private NtStatus ReadFile(BuildInfoPath path, byte[] buffer, out int bytesRead, long offset) =>
            ReadBuffer(_mongoDb.GetBuildInfo().AsJsonBytes(), buffer, out bytesRead, offset);

        private NtStatus ReadFile(HostInfoPath path, byte[] buffer, out int bytesRead, long offset) =>
            ReadBuffer(_mongoDb.GetHostInfo().AsJsonBytes(), buffer, out bytesRead, offset);

        private NtStatus ReadFile(ListCommandsPath path, byte[] buffer, out int bytesRead, long offset) =>
            ReadBuffer(_mongoDb.GetListCommands().AsJsonBytes(), buffer, out bytesRead, offset);

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

        private NtStatus ReadFile(QueryDocumentPath path, byte[] buffer, out int bytesRead, long offset)
        {
            var document = _mongoDb.QueryDocumentAt(path.Database, path.Collection, path.Index, path.Field, path.Query);
            if (document == null)
            {
                bytesRead = 0;
                return NtStatus.NoSuchFile;
            }

            var sourceBuffer = path.Type switch
            {
                DataDocumentType.Json => document.AsJsonBytes(),
                DataDocumentType.Bson => document.AsBsonBytes(),
                _ => throw new ArgumentOutOfRangeException($"Unsupported path type {path.Type}")
            };

            return ReadBuffer(sourceBuffer, buffer, out bytesRead, offset);
        }

        private NtStatus ReadFile(QueryAllDocumentsPath path, byte[] buffer, out int bytesRead, long offset)
        {
            var documents = _mongoDb.QueryAllDocuments(path.Database, path.Collection, path.Field, path.Query);
            var container = new BsonContainer<BsonArray>(new BsonArray(documents.Select(d => d.Value)));

            var sourceBuffer = container.AsJsonBytes();
            return ReadBuffer(sourceBuffer, buffer, out bytesRead, offset);
        }

        private NtStatus ReadFile(QueryDirectoryPath path, byte[] buffer, out int bytesRead, long offset)
        {
            bytesRead = 0;
            return NtStatus.NoSuchFile;
        }

        private NtStatus ReadBuffer(byte[] source, byte[] destination, out int bytesRead, long offset)
        {
            if (offset > source.Length)
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
