using DokanNet;
using MongoDB.Bson;
using MongoFs.Extensions;
using MongoFs.Paths.Root.Database.Collection.Data;
using MongoFs.Paths.Root.Database.Collection.Query;
using MongoFs.Paths.Root.Database.Collection;
using MongoFs.Paths.Root.Database;
using MongoFs.Paths.Root;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System;
using Path = MongoFs.Paths.Abstract.Path;


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
                RootPath p                => FindFilesWithPattern(p, searchPattern, out files),
                DatabasePath p            => FindFilesWithPattern(p, searchPattern, out files),
                CollectionPath p          => FindFilesWithPattern(p, searchPattern, out files),
                DataDirectoryPath p       => FindFilesWithPattern(p, searchPattern, out files),
                QueryDirectoryPath p      => FindFilesWithPattern(p, searchPattern, out files),
                QueryEmptyDirectoryPath p => FindFilesWithPattern(p, searchPattern, out files),

                var p                     => LogFailure(path)
            };
        }

        private NtStatus FindFilesWithPattern(RootPath _, string? searchPattern, out IList<FileInformation> files)
        {
            files = _mongoDb.GetDatabases().Select(db => new FileInformation
            {
                FileName = db,
                Attributes = MongoDir,
                CreationTime = DateTime.Now,
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
                Attributes = MongoDir,
                CreationTime = DateTime.Now,
            })
            .FilterWithGlobPattern(searchPattern)
            .ToList();
            return NtStatus.Success;
        }

        private NtStatus FindFilesWithPattern(CollectionPath path, string? searchPattern, out IList<FileInformation> files)
        {
            var now = DateTime.Now;
            files = new List<FileInformation>
            {
                new FileInformation
                {
                    FileName = StatsPath.FileName,
                    Attributes = MongoFile,
                    CreationTime = now,
                    Length = _mongoDb.GetStats(path.Database, path.Collection).GetJsonBytesLength()
                },
                new FileInformation
                {
                    FileName = IndexesPath.FileName,
                    Attributes = MongoFile,
                    CreationTime = now,
                    Length = _mongoDb.GetIndexes(path.Database, path.Collection).GetJsonBytesLength()
                },
                new FileInformation
                {
                    FileName = DataDirectoryPath.FileName,
                    Attributes = MongoDir,
                    CreationTime = now
                },
                new FileInformation
                {
                    FileName = QueryDirectoryPath.FileName,
                    Attributes = MongoDir,
                    CreationTime = now
                }
            };

            if (searchPattern != null)
            {
                files = files.FilterWithGlobPattern(searchPattern).ToList();
            }
            return NtStatus.Success;
        }

        private NtStatus FindFilesWithPattern(DataDirectoryPath path, string? searchPattern, out IList<FileInformation> files)
        {
            var now = DateTime.Now;

            files = _mongoDb
                .GetAllDocuments(path.Database, path.Collection)
                .WithIndexes()
                .Select(document =>
                {
                    var index = document.Index.ToString();
                    var creationTime = document.Item.Value.TryGetCreationTimeFromId() ?? now;

                    return
                        (
                            new FileInformation
                            {
                                FileName = $"{index}.json",
                                Attributes = MongoFile,
                                CreationTime = creationTime,
                                Length = document.Item.GetJsonBytesLength(),
                            },
                            new FileInformation
                            {
                                FileName = $"{index}.bson",
                                Attributes = MongoFile,
                                CreationTime = creationTime,
                                Length = document.Item.GetBsonBytesLength()
                            }
                        );
                })
                .Untuple()
                .FilterWithGlobPattern(searchPattern)
                .ToList();

            return NtStatus.Success;
        }

        private NtStatus FindFilesWithPattern(QueryDirectoryPath path, string? searchPattern, out IList<FileInformation> files)
        {
            var now = DateTime.Now;

            var documents = _mongoDb
                .QueryAllDocuments(path.Database, path.Collection, path.Field, path.Query)
                .ToList();

            var allItems = new BsonContainer<BsonArray>(new BsonArray(documents.Select(d => d.Value)));

            files = documents
                .WithIndexes()
                .Select(document =>
                {
                    var index = document.Index.ToString();
                    var creationTime = document.Item.Value.TryGetCreationTimeFromId() ?? now;

                    return
                        (
                            new FileInformation
                            {
                                FileName = $"{index}.json",
                                Attributes = MongoFile,
                                CreationTime = creationTime,
                                Length = document.Item.GetJsonBytesLength()
                            },
                            new FileInformation
                            {
                                FileName = $"{index}.bson",
                                Attributes = MongoFile,
                                CreationTime = creationTime,
                                Length = document.Item.GetBsonBytesLength()
                            }
                        );
                })
                .Untuple()
                .Prepend(new FileInformation
                {
                    FileName = QueryAllDocumentsPath.FileName,
                    CreationTime = DateTime.Now,
                    Attributes = MongoFile,
                    Length = allItems.GetJsonBytesLength()
                })
                .FilterWithGlobPattern(searchPattern)
                .ToList();

            return NtStatus.Success;
        }

        private NtStatus FindFilesWithPattern(QueryEmptyDirectoryPath path, string? searchPattern, out IList<FileInformation> files)
        {
            files = new List<FileInformation>();
            return NtStatus.Success;
        }

    }
}
