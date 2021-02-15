using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;
using MongoFs.Extensions;
using MongoFs.Paths;
using Serilog;
using Path = MongoFs.Paths.Path;

namespace MongoFs.Handlers
{
    public class FindFilesWithPatternHandler : BaseHandler
    {
        public FindFilesWithPatternHandler
            (
                ILogger logger,
                IMongoDb mongoDb
            ) : base(logger, mongoDb)
        {
        }

        public NtStatus FindFilesWithPattern(Path path, string? searchPattern, out IList<FileInformation> files,
            IDokanFileInfo info)
        {
            files = Array.Empty<FileInformation>();
            return path switch
            {
                RootPath p => FindFilesWithPattern(p, searchPattern, out files),
                DatabasePath p => FindFilesWithPattern(p, searchPattern, out files),
                CollectionPath p => FindFilesWithPattern(p, searchPattern, out files),

                var p => LogFailure(path)
            };
        }

        private NtStatus FindFilesWithPattern(RootPath _, string? searchPattern, out IList<FileInformation> files)
        {
            files = _mongoDb.GetDatabases().Select(db => new FileInformation
            {
                FileName = db,
                Attributes = FileAttributes.Directory | FileAttributes.ReadOnly,
                CreationTime = DateTime.Now,
                Length = _mongoDb.GetDatabaseSize(db)
            })
            .FilterWithGlobPattern(searchPattern)
            .ToList();
            return NtStatus.Success;
        }

        private NtStatus FindFilesWithPattern(DatabasePath path, string? searchPattern, out IList<FileInformation> files)
        {
            files = _mongoDb.GetCollections(path.Database).Select(coll => new FileInformation
            {
                FileName = coll,
                Attributes = FileAttributes.Directory | FileAttributes.ReadOnly,
                CreationTime = DateTime.Now,
                Length = _mongoDb.GetCollectionSize(path.Database, coll)
            })
            .FilterWithGlobPattern(searchPattern)
            .ToList();
            return NtStatus.Success;
        }

        private NtStatus FindFilesWithPattern(CollectionPath path, string? searchPattern, out IList<FileInformation> files)
        {
            files = new List<FileInformation>
            {
                new FileInformation
                {
                    FileName = StatsPath.FileName,
                    Attributes = FileAttributes.Normal | FileAttributes.ReadOnly,
                    CreationTime = DateTime.Now,
                    Length = _mongoDb.GetStats(path.Database, path.Collection).GetJsonBytesLength()
                },
                new FileInformation
                {
                    FileName = IndexesPath.FileName,
                    Attributes = FileAttributes.Normal | FileAttributes.ReadOnly,
                    CreationTime = DateTime.Now,
                    Length = _mongoDb.GetIndexes(path.Database, path.Collection).GetJsonBytesLength()
                }
            };

            if (searchPattern != null)
            {
                files = files.FilterWithGlobPattern(searchPattern).ToList();
            }
            return NtStatus.Success;
        }
    }
}
