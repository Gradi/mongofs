using System;
using System.IO;
using DokanNet;
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
                RootPath p => GetFileInformation(p, out fileInfo),
                DatabasePath p => GetFileInformation(p, out fileInfo),
                CollectionPath p => GetFileInformation(p, out fileInfo),

                StatsPath p => GetFileInformation(p, out fileInfo),
                IndexesPath p => GetFileInformation(p, out fileInfo),

                var p => LogFailure(p)
            };
        }

        private NtStatus GetFileInformation(RootPath _, out FileInformation fileInfo)
        {
            fileInfo = new FileInformation
            {
                FileName = System.IO.Path.DirectorySeparatorChar.ToString(),
                Attributes = FileAttributes.Directory | FileAttributes.ReadOnly,
                CreationTime = DateTime.Now,
                Length = _mongoDb.GetTotalDbSize()
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
                Length = _mongoDb.GetDatabaseSize(path.Database)
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
                Length = _mongoDb.GetCollectionSize(path.Database, path.Collection)
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
    }
}
