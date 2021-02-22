using System.Runtime.CompilerServices;
using DokanNet;
using MongoFs.Paths;
using Serilog;
using Serilog.Events;

namespace MongoFs.Handlers
{
    public abstract class BaseHandler
    {
        protected readonly ILogger _logger;
        protected readonly IMongoDb _mongoDb;

        protected BaseHandler
            (
                ILogger logger,
                IMongoDb mongoDb
            )
        {
            _logger = logger.ForContext(GetType());
            _mongoDb = mongoDb;
        }

        protected NtStatus LogFailure(Path path, [CallerMemberName]string? callerName = null)
        {
            if (_logger.IsEnabled(LogEventLevel.Information))
            {
                _logger.Information("Handler for combination ({MethodName}, {PathType}) is not implemented.",
                    callerName, path.GetType());
            }
            return NtStatus.NotImplemented;
        }
    }
}