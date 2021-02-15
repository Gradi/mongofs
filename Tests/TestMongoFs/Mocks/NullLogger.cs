using Serilog;
using Serilog.Events;

namespace TestMongoFs.Mocks
{
    public class NullLogger : ILogger
    {
        public void Write(LogEvent logEvent) {}
    }
}
