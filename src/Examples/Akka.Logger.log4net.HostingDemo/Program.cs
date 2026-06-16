// Akka.Logger.log4net Hosting Demo
// Demonstrates wiring log4net as the Akka.NET logger via Akka.Hosting.

using Akka.Actor;
using Akka.Hosting;
using Akka.Logger.log4net;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// --- Configure log4net programmatically ---
// Use a console appender with a pattern that shows the Akka log source.
var layout = new PatternLayout(
    "%date{HH:mm:ss.fff} [%-5level] [%property{akka.logSource}] %message%newline%exception");
layout.ActivateOptions();

var consoleAppender = new ConsoleAppender { Layout = layout };
consoleAppender.ActivateOptions();

BasicConfigurator.Configure(consoleAppender);

// Set the root logger to DEBUG so all Akka log events come through.
var root = ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root;
root.Level = log4net.Core.Level.Debug;
root.Repository.Configured = true;

// --- Build and run the hosted application ---
var host = Host.CreateDefaultBuilder(args)
    // Silence Microsoft's generic host console output so only log4net lines appear.
    .ConfigureLogging(lb => lb.ClearProviders())
    .ConfigureServices((_, services) =>
    {
        services.AddAkka("DemoSystem", cb =>
        {
            cb.ConfigureLoggers(lc =>
            {
                // Remove the default Akka console logger and route all
                // Akka log events through log4net instead.
                lc.ClearLoggers();
                lc.AddLogger<Log4NetLogger>();
                // Use the fully-qualified Akka LogLevel to avoid ambiguity
                // with Microsoft.Extensions.Logging.LogLevel.
                lc.LogLevel = Akka.Event.LogLevel.DebugLevel;
            });

            // Register a simple actor that emits log messages on startup.
            cb.WithActors((system, registry) =>
            {
                system.ActorOf(Props.Create(() => new DemoActor()), "demo");
            });
        });
    })
    .Build();

await host.StartAsync();

// Give the demo actor a moment to log its messages through the async pipeline.
await Task.Delay(1500);

await host.StopAsync();

// --- Demo actor ---
// ILoggingAdapter in Akka 1.5.x exposes Log(LogLevel, Exception?, string) as
// its primary logging method on the interface. Use fully-qualified names to
// avoid ambiguity with Microsoft.Extensions.Logging.LogLevel.
public sealed class DemoActor : ReceiveActor
{
    private readonly Akka.Event.ILoggingAdapter _log = Akka.Event.Logging.GetLogger(
        Context.System.EventStream, typeof(DemoActor).FullName!);

    public DemoActor()
    {
        _log.Log(Akka.Event.LogLevel.InfoLevel, null,
            "DemoActor started - log4net integration is working via Akka.Hosting.");

        _log.Log(Akka.Event.LogLevel.DebugLevel, null,
            $"Debug message: actor path is {Self.Path}");

        _log.Log(Akka.Event.LogLevel.WarningLevel, null,
            "Warning example: parameterized message Name=log4net Count=42");

        _log.Log(Akka.Event.LogLevel.ErrorLevel, null,
            "Error example: demonstrating error-level routing through the log4net adapter.");
    }
}
