using DokanNet;
using MongoDB.Bson;
using MongoFs.Extensions;
using MongoFs.Paths.Root.Database.Collection.Data;
using MongoFs.Paths.Root.Database.Collection.Query;
using MongoFs.Paths.Root.Database.Collection;
using MongoFs.Paths.Root.Database;
using MongoFs.Paths.Root;
using MongoFs.Paths;
using Serilog;
using System.Linq;
using System.Text;
using System;
using Path = MongoFs.Paths.Abstract.Path;

namespace MongoFs.Handlers
{
    public class GetFileInformationHandler : BaseHandler
    {
        public GetFileInformationHandler
            (
                ILogger logger,
                IMongoDb mongoDb
            ) : base(logger, mongoDb)
        {
        }

        public NtStatus GetFileInformation(Path path, out FileInformation fileInfo, IDokanFileInfo info)
        {
            fileInfo = default;
            return path switch
            {
                // Dirs
                RootPath p                => GetFileInformation(p, out fileInfo),
                DatabasePath p            => GetFileInformation(p, out fileInfo),
                CollectionPath p          => GetFileInformation(p, out fileInfo),
                DataDirectoryPath p       => GetFileInformation(p, out fileInfo),
                QueryDirectoryPath p      => GetFileInformation(p, out fileInfo),
                QueryEmptyDirectoryPath p => GetFileInformation(p, out fileInfo),

                // Files
                StatsPath p               => GetFileInformation(p, out fileInfo),
                IndexesPath p             => GetFileInformation(p, out fileInfo),
                DataDocumentPath p        => GetFileInformation(p, out fileInfo),
                QueryDocumentPath p       => GetFileInformation(p, out fileInfo),
                QueryAllDocumentsPath p   => GetFileInformation(p, out fileInfo),

                var p                => LogFailure(p)
            };
        }

        private NtStatus GetFileInformation(RootPath _, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = System.IO.Path.DirectorySeparatorChar.ToString(),
                Attributes = MongoDir,
                CreationTime = DateTime.Now,
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(DatabasePath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = path.Database,
                Attributes = MongoDir,
                CreationTime = DateTime.Now,
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(CollectionPath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = path.Collection,
                Attributes = MongoDir,
                CreationTime = DateTime.Now,
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(StatsPath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = StatsPath.FileName,
                Attributes = MongoFile,
                CreationTime = DateTime.Now,
                Length = _mongoDb.GetStats(path.Database, path.Collection).GetJsonBytesLength()
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(IndexesPath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = IndexesPath.FileName,
                Attributes = MongoFile,
                CreationTime = DateTime.Now,
                Length = _mongoDb.GetIndexes(path.Database, path.Collection).GetJsonBytesLength()
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(DataDirectoryPath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = DataDirectoryPath.FileName,
                Attributes = MongoDir,
                CreationTime = DateTime.Now,
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(DataDocumentPath path, out FileInformation fileInfo)
        {
            var document = _mongoDb.GetDocumentAt(path.Database, path.Collection, path.Index);
            if (document == null)
            {
                fileInfo = default;
                return NtStatus.NoSuchFile;
            }

            fileInfo = new FileInformation
            {
                FileName = path.FileName,
                Attributes = MongoFile,
                CreationTime = document.Value.TryGetCreationTimeFromId() ?? DateTime.Now,
                Length = path.Type switch
                {
                    DataDocumentType.Json => document.GetJsonBytesLength(),
                    DataDocumentType.Bson => document.GetBsonBytesLength(),
                    _ => throw new ArgumentOutOfRangeException($"Unsupported path type {path.Type}")
                }
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(QueryDirectoryPath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = QueryDirectoryPath.FileName,
                Attributes = MongoDir,
                CreationTime = DateTime.Now
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(QueryEmptyDirectoryPath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = QueryDirectoryPath.FileName,
                Attributes = MongoDir,
                CreationTime = DateTime.Now
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(QueryDocumentPath path, out FileInformation fileInfo)
        {
            var document = _mongoDb.QueryDocumentAt(path.Database, path.Collection, path.Index, path.Field, path.Query);
            if (document == null)
            {
                fileInfo = default;
                return NtStatus.NoSuchFile;
            }

            fileInfo = new FileInformation
            {
                FileName = path.FileName,
                Attributes = MongoFile,
                CreationTime = document.Value.TryGetCreationTimeFromId() ?? DateTime.Now,
                Length = path.Type switch
                {
                    DataDocumentType.Json => document.GetJsonBytesLength(),
                    DataDocumentType.Bson => document.GetBsonBytesLength(),
                    _ => throw new ArgumentOutOfRangeException($"Unsupported path type {path.Type}.")
                }
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(QueryAllDocumentsPath path, out FileInformation fileInfo)
        {
            var documents = _mongoDb.QueryAllDocuments(path.Database, path.Collection, path.Field, path.Query);
            var container = new BsonContainer<BsonValue>(new BsonArray(documents.Select(d => d.Value)));

            fileInfo = new FileInformation
            {
                FileName = QueryAllDocumentsPath.FileName,
                Attributes = MongoFile,
                CreationTime = DateTime.Now,
                Length = container.GetJsonBytesLength()
            };
            return NtStatus.Success;
        }
    }
}
