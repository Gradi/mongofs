using System;
using System.IO;
using DokanNet;
using MongoFs.Extensions;
using MongoFs.Paths;
using Serilog;
using Path = MongoFs.Paths.Path;

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
                RootPath p           => GetFileInformation(p, out fileInfo),
                DatabasePath p       => GetFileInformation(p, out fileInfo),
                CollectionPath p     => GetFileInformation(p, out fileInfo),
                DataDirectoryPath p  => GetFileInformation(p, out fileInfo),

                // Files
                StatsPath p          => GetFileInformation(p, out fileInfo),
                IndexesPath p        => GetFileInformation(p, out fileInfo),
                DataDocumentPath p   => GetFileInformation(p, out fileInfo),

                var p                => LogFailure(p)
            };
        }

        private NtStatus GetFileInformation(RootPath _, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = System.IO.Path.DirectorySeparatorChar.ToString(),
                Attributes = FileAttributes.Directory | FileAttributes.ReadOnly,
                CreationTime = DateTime.Now,
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(DatabasePath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = path.Database,
                Attributes = FileAttributes.Directory | FileAttributes.ReadOnly,
                CreationTime = DateTime.Now,
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(CollectionPath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = path.Collection,
                Attributes = FileAttributes.Directory | FileAttributes.ReadOnly,
                CreationTime = DateTime.Now,
            };
            return NtStatus.Success;
        }

        private NtStatus GetFileInformation(StatsPath path, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = StatsPath.FileName,
                Attributes = FileAttributes.Normal | FileAttributes.ReadOnly,
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
                Attributes = FileAttributes.Normal | FileAttributes.ReadOnly,
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
                Attributes = FileAttributes.Directory | FileAttributes.ReadOnly,
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
                Attributes = FileAttributes.Normal | FileAttributes.ReadOnly,
                CreationTime = document.BsonDocument.TryGetCreationTimeFromId() ?? DateTime.Now,
                Length = path.Type switch
                {
                    DataDocumentType.Json => document.GetJsonBytesLength(),
                    DataDocumentType.Bson => document.GetBsonBytesLength(),
                    _ => throw new ArgumentOutOfRangeException($"Unsupported path type {path.Type}")
                }
            };
            return NtStatus.Success;
        }
    }
}
