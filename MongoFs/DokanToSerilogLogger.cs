using Serilog;

namespace MongoFs
{
    public class DokanToSerilogLogger : DokanNet.Logging.ILogger
    {
        private readonly ILogger _logger;

        public DokanToSerilogLogger
            (
                ILogger logger
            )
        {
            _logger = logger.ForContext<DokanToSerilogLogger>();
        }

        public void Debug(string message, params object[] args) =>
            _logger.Debug(message, args);

        public void Info(string message, params object[] args) =>
            _logger.Information(message, args);

        public void Warn(string message, params object[] args) =>
            _logger.Warning(message, args);

        public void Error(string message, params object[] args) =>
            _logger.Error(message, args);

        public void Fatal(string message, params object[] args) =>
            _logger.Fatal(message, args);
    }
}
