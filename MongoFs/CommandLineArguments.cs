using System;
using System.Collections.Generic;
using CommandLine;
using MongoDB.Driver;
using Serilog.Events;

namespace MongoFs
{
    public class CommandLineArguments
    {
        [Option("log-file", HelpText = "Path to a log file.")]
        public string? LogFile { get; set; }

        [Option("log-console", HelpText = "Enable console logging", Default = false)]
        public bool IsConsoleLoggingEnabled { get; set; }

        [Option("log-level", HelpText = "Log level", Default = LogEventLevel.Information)]
        public LogEventLevel LogLevel { get; set; }

        [Option('c', "connection-string", HelpText = "Connection string to MongoDb", Required = true)]
        public string ConnectionString { get; set; } = string.Empty;

        [Option('n', "name", HelpText = "Name of a MongoDb instance.", Required = true)]
        public string Name { get; set; } = string.Empty;

        [Option('p', "path", HelpText = "Mount point (M:\\) or mount path (C:\\mongodb). Note the ending slash.", Required = true)]
        public string MountPoint { get; set; } = string.Empty;

        public void Validate()
        {
            var errors = new List<Exception>();
            try
            {
                MongoUrl.Create(ConnectionString);
            }
            catch(Exception exception)
            {
                errors.Add(new Exception($"Invalid connection string: {ConnectionString}", exception));
            }

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(new Exception("Name is null or empty."));

            if (string.IsNullOrWhiteSpace(MountPoint))
                errors.Add(new Exception("Mount poing is null or empty."));

            if (errors.Count != 0)
                throw new AggregateException("Command line options are invalid.", errors);
        }
    }
}
