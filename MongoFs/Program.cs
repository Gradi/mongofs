using System.Text;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoFs.Handlers;
using MongoFs.Paths;
using Serilog;

namespace MongoFs
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<CommandLineArguments>(args)
                .WithParsed(NewMain);
        }

        private static void NewMain(CommandLineArguments args)
        {
            args.Validate();
            using var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(CommandLineArguments args)
        {
            var builder = new HostBuilder();
            builder.UseDefaultServiceProvider(sp =>
            {
                sp.ValidateScopes = true;
                sp.ValidateOnBuild = true;
            })
            .UseConsoleLifetime()
            .ConfigureLogging(log => ConfigureSerilog(log, args))
            .ConfigureServices(services =>
            {
                services.AddSingleton(args);

                services.AddSingleton<DokanNet.IDokanOperations, MongoDbDokanOperations>();
                services.AddSingleton<DokanNet.Logging.ILogger, DokanToSerilogLogger>();

                services.AddSingleton<IMongoDb>(sp => new MongoDb(sp.GetRequiredService<CommandLineArguments>().ConnectionString));
                services.AddSingleton<IPathParser>(sp => new PathParser(sp.GetRequiredService<Serilog.ILogger>(), System.IO.Path.DirectorySeparatorChar));

                services.AddSingleton<FindFilesWithPatternHandler>();
                services.AddSingleton<GetFileInformationHandler>();
                services.AddSingleton<ReadFileHandler>();

                services.AddHostedService<HostedMongoFsService>();
            });

            return builder;
        }

        private static void ConfigureSerilog(ILoggingBuilder log, CommandLineArguments args)
        {
            var config = new LoggerConfiguration();
            config.MinimumLevel.Is(args.LogLevel);

            if (args.IsConsoleLoggingEnabled)
                config.WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(args.LogFile))
                config.WriteTo.File(args.LogFile, shared: true, encoding: Encoding.UTF8);

            Serilog.Log.Logger = config.CreateLogger();
            log.Services.AddSingleton<Serilog.ILogger>(Serilog.Log.Logger);

            log.ClearProviders();
            log.SetMinimumLevel(LogLevel.Trace);
            log.AddSerilog();
        }
    }
}
