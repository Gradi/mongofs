using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoFs.Extensions;
using MongoFs.Paths.Abstract;
using MongoFs.Paths.Root;
using MongoFs.Paths.Root.Database;
using MongoFs.Paths.Root.Database.Collection;
using MongoFs.Paths.Root.Database.Collection.Data;
using MongoFs.Paths.Root.Database.Collection.Query;
using Serilog;

namespace MongoFs.Paths
{
    public class PathParser : IPathParser
    {
        private const string BadDbColl = "Invalid database or collection name.";
        private const string Id = "_id";

        private readonly ILogger _logger;
        private readonly char _directorySeparator;

        private readonly IReadOnlyDictionary<int, Func<string, string[], Path?>> _parsers;

        public PathParser
            (
                ILogger logger,
                char directorySeparator
            )
        {
            _logger = logger.ForContext<PathParser>();
            _directorySeparator = directorySeparator;

            _parsers = FindParsers();
        }

        public Path? Parse(string? pathStr)
        {
            if (string.IsNullOrEmpty(pathStr))
                return LogFailure(pathStr, "Path is null or empty.");
            if (pathStr[0] != _directorySeparator)
                return LogFailure(pathStr, "Path is not absolute.");

            if (pathStr.Length == 1 && pathStr[0] == _directorySeparator)
                return new RootPath();

            var segments = pathStr.Split(_directorySeparator, StringSplitOptions.RemoveEmptyEntries);
            if (_parsers.TryGetValue(segments.Length, out var parser))
                return parser(pathStr, segments);

            return LogFailure(pathStr);
        }

        // /database
        private Path? ParseDatabasePath(string inputStr, string database) =>
            database.IsValidDbOrCollName() ? new DatabasePath(database) : LogFailure(inputStr, "Invalid database name.");

        // /database/collection
        private Path? ParseCollectionPath(string inputStr, string database, string collection) =>
            database.IsValidDbOrCollName() && collection.IsValidDbOrCollName() ?
                new CollectionPath(database, collection) : LogFailure(inputStr, BadDbColl);

        // /database/collection
        //     /stats.json
        //     /indexes.json
        //     /data/
        //     /query/
        private Path? Parse3SegmentsPath(string inputStr, string database, string collection, string segment)
        {
            if (!database.IsValidDbOrCollName() || !collection.IsValidDbOrCollName())
                return LogFailure(inputStr, BadDbColl);

            return segment switch
            {
                StatsPath.FileName => new StatsPath(database, collection),
                IndexesPath.FileName => new IndexesPath(database, collection),
                DataDirectoryPath.FileName => new DataDirectoryPath(database, collection),
                QueryDirectoryPath.FileName => new QueryEmptyDirectoryPath(database, collection),
                _ => LogFailure(inputStr)
            };
        }

        // /database/collection/
        //     data/<numeric-index>.[bson|json]
        //     query/<query>
        private Path? Parse4SegmentsPath(string inputStr, string database, string collection, string thirdSegment, string fourthSegment)
        {
            if (!database.IsValidDbOrCollName() || !collection.IsValidDbOrCollName())
                return LogFailure(inputStr, BadDbColl);

            return thirdSegment switch
            {
                DataDirectoryPath.FileName when fourthSegment.IsValidDocumentIndex(out var index, out var type) =>
                    new DataDocumentPath(database, collection, index, type),

                QueryDirectoryPath.FileName => new QueryDirectoryPath(database, collection, fourthSegment, Id),

                _ => LogFailure(inputStr)
            };
        }

        // /database/collection
        //     /query/<query>/<numeric-index>.[json|bson]
        //     /query/<query>/all.json
        //     /query/<field>/<query>
        private Path? Parse5SegmentsPath(string inputStr, string database, string collection, string thirdSegment,
                                         string fourthSegment, string fifthSegment)
        {
            if (!database.IsValidDbOrCollName() || !collection.IsValidDbOrCollName())
                return LogFailure(inputStr, BadDbColl);

            return thirdSegment switch
            {
                QueryDirectoryPath.FileName when fifthSegment.IsValidDocumentIndex(out var index, out var type) =>
                    new QueryDocumentPath(database, collection, fourthSegment, Id, index, type),

                QueryDirectoryPath.FileName when fifthSegment == QueryAllDocumentsPath.FileName =>
                    new QueryAllDocumentsPath(database, collection, fourthSegment, Id),

                QueryDirectoryPath.FileName =>
                    new QueryDirectoryPath(database, collection, fifthSegment, fourthSegment),

                _ => LogFailure(inputStr)
            };
        }

        // /database/collection/query/<field name>
        //     /<query>/<numeric index>.[json|bson]
        //     /<query>/all.json
        private Path? Parse6SegmentsPath(string inputStr, string database, string collection, string thirdSegment,
                                         string fourthSegment, string fifthSegment, string sixthSegment)
        {
            if (!database.IsValidDbOrCollName() || !collection.IsValidDbOrCollName())
                LogFailure(inputStr, BadDbColl);

            return thirdSegment switch
            {
                QueryDirectoryPath.FileName when sixthSegment.IsValidDocumentIndex(out var index, out var type) =>
                    new QueryDocumentPath(database, collection, fifthSegment, fourthSegment, index, type),

                QueryDirectoryPath.FileName when sixthSegment == QueryAllDocumentsPath.FileName =>
                    new QueryAllDocumentsPath(database, collection, fifthSegment, fourthSegment),

                _ => LogFailure(inputStr)
            };
        }

        private Path? LogFailure(string? inputPath, string? reason = null)
        {
            _logger.Verbose("Can't parse {InputPath} due to {Reason}", inputPath, reason ?? "Unrecognized path.");
            return null;
        }

        private IReadOnlyDictionary<int, Func<string, string[], Path?>> FindParsers()
        {
            // Looking for methods like
            // private Path? ParseSomething(string inputPath, string segment1, string segment2, ..., string segmentN);
            var methods = typeof(PathParser)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name.StartsWith("Parse") && m.ReturnType == typeof(Path))
                .Where(m => m.GetParameters().All(p => p.ParameterType == typeof(string)))
                .ToList();

            var result = new Dictionary<int, Func<string, string[], Path?>>();
            foreach (var method in methods)
            {
                var args = method.GetParameters();
                var key = args.Length - 1;
                if (result.ContainsKey(key))
                {
                    throw new Exception($"{typeof(PathParser)} contains more than 1 parser method with the same number" +
                                        $" of segment arguments ({method.Name})");
                }

                var inputStr = Expression.Parameter(typeof(string));
                var segments = Expression.Parameter(typeof(string[]));

                var inputParams = Enumerable.Range(0, args.Length - 1)
                    .Select(i => (Expression)Expression.ArrayAccess(segments, Expression.Constant(i)))
                    .Prepend(inputStr);

                Expression body = Expression.Call(Expression.Constant(this), method, inputParams);

                var lambda = Expression.Lambda<Func<string, string[], Path?>>(body, inputStr, segments);
                result.Add(key, lambda.Compile());
            }
            return result;
        }
    }
}
