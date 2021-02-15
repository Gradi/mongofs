using System;
using System.Threading;
using System.Threading.Tasks;
using DokanNet;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MongoFs
{
    public class HostedMongoFsService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly CommandLineArguments _commandLineArguments;
        private readonly IHostApplicationLifetime _lifetime;

        private readonly IDokanOperations _dokanOperations;
        private readonly DokanNet.Logging.ILogger _dokanLogger;

        public HostedMongoFsService
            (
                ILogger logger,
                CommandLineArguments commandLineArguments,
                IHostApplicationLifetime lifetime,
                IDokanOperations dokanOperations,
                DokanNet.Logging.ILogger dokanLogger
            )
        {
            _logger = logger.ForContext<HostedMongoFsService>();
            _commandLineArguments = commandLineArguments;
            _lifetime = lifetime;
            _dokanOperations = dokanOperations;
            _dokanLogger = dokanLogger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Mounting MongoDb@{ConnectionString} as {MountPoint}",
                _commandLineArguments.ConnectionString, _commandLineArguments.MountPoint);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    _dokanOperations.Mount
                        (
                            _commandLineArguments.MountPoint,
                            DokanOptions.WriteProtection,
                            _dokanLogger
                        );
                }
                catch(Exception exception)
                {
                    _logger.Fatal(exception, "Exception on mouting filesystem.");
                    _lifetime.StopApplication();
                }
            }, TaskCreationOptions.LongRunning);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (!Dokan.RemoveMountPoint(_commandLineArguments.MountPoint))
            {
                _logger.Fatal("Can't unmount {MountPoint}", _commandLineArguments.MountPoint);
            }
            return Task.CompletedTask;
        }
    }
}
