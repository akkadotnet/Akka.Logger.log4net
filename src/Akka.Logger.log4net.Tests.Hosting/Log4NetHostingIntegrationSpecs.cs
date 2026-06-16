// Integration tests that prove Akka.Hosting correctly wires log4net as the
// Akka logger and that log events actually travel through the adapter into
// a log4net MemoryAppender.

using Akka.Actor;
using Akka.Event;
using Akka.Hosting;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

// Explicit alias to avoid ambiguity with Microsoft.Extensions.Logging.LogLevel
using AkkaLogLevel = Akka.Event.LogLevel;

namespace Akka.Logger.log4net.Tests.Hosting;

/// <summary>
/// End-to-end test: boot an ActorSystem via Akka.Hosting with
/// AddLogger&lt;Log4NetLogger&gt;() and verify that log events emitted by the
/// Akka logging infrastructure actually reach a log4net MemoryAppender.
/// </summary>
/// <remarks>
/// Log4NetLogger calls LogManager.GetLogger(logEvent.LogClass) which resolves
/// to the default log4net repository.  The MemoryAppender must therefore be
/// attached to the default repository's root logger, not a named one.
/// </remarks>
public sealed class Log4NetHostingIntegrationSpecs : IAsyncLifetime
{
    private IHost? _host;
    private MemoryAppender? _memoryAppender;
    private Hierarchy? _hierarchy;

    public async Task InitializeAsync()
    {
        // Attach a MemoryAppender to the DEFAULT log4net repository root
        // because Log4NetLogger routes events through LogManager.GetLogger()
        // which resolves to the default repository.
        _hierarchy = (Hierarchy)LogManager.GetRepository();
        _hierarchy.Root.Level = global::log4net.Core.Level.Debug;

        _memoryAppender = new MemoryAppender();
        _memoryAppender.ActivateOptions();

        // Add the appender to the root logger of the default repository.
        _hierarchy.Root.AddAppender(_memoryAppender);
        _hierarchy.Configured = true;

        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging(lb => lb.ClearProviders())
            .ConfigureServices((_, services) =>
            {
                services.AddAkka("TestSystem", cb =>
                {
                    cb.ConfigureLoggers(lc =>
                    {
                        lc.ClearLoggers();
                        lc.AddLogger<Log4NetLogger>();
                        lc.LogLevel = AkkaLogLevel.DebugLevel;
                    });
                });
            })
            .Build();

        await _host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        // Remove the MemoryAppender to avoid polluting other tests.
        if (_memoryAppender is not null && _hierarchy is not null)
        {
            _hierarchy.Root.RemoveAppender(_memoryAppender);
            _memoryAppender.Clear();
        }
    }

    [Fact]
    public async Task AddLogger_Log4NetLogger_routes_Akka_log_events_through_log4net()
    {
        // Arrange: obtain the ActorSystem from DI and emit a log message.
        var actorSystem = _host!.Services.GetRequiredService<ActorSystem>();

        const string expectedMessage = "Hosting integration test message from Akka";

        var logger = Logging.GetLogger(actorSystem.EventStream, typeof(Log4NetHostingIntegrationSpecs).FullName!);

        // Act: publish a log event into the Akka event stream.
        // ILoggingAdapter in Akka 1.5.x exposes Log(LogLevel, Exception?, string)
        // as its core interface method.
        logger.Log(AkkaLogLevel.InfoLevel, null, expectedMessage);

        // The Akka logger pipeline is asynchronous: the event travels from the
        // caller -> EventStream -> Log4NetLogger actor mailbox -> log4net.
        // Poll the MemoryAppender for up to 5 seconds to allow delivery.
        global::log4net.Core.LoggingEvent[]? capturedEvents = null;
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            capturedEvents = _memoryAppender!.GetEvents();
            if (capturedEvents.Any(e => e.RenderedMessage.Contains(expectedMessage)))
                break;

            await Task.Delay(50);
        }

        capturedEvents = _memoryAppender!.GetEvents();

        // Assert: at least one event was captured and it contains our message.
        capturedEvents.Should().NotBeNullOrEmpty(
            "the Log4NetLogger actor should have forwarded the Akka log event to log4net");

        capturedEvents!.Should().Contain(
            e => e.RenderedMessage.Contains(expectedMessage),
            $"the captured events should include the message '{expectedMessage}'");
    }

    [Fact]
    public async Task AddLogger_Log4NetLogger_captures_warning_level_events()
    {
        var actorSystem = _host!.Services.GetRequiredService<ActorSystem>();

        const string warningMessage = "This is a warning from Akka via log4net hosting integration";

        var logger = Logging.GetLogger(actorSystem.EventStream, typeof(Log4NetHostingIntegrationSpecs).FullName!);
        logger.Log(AkkaLogLevel.WarningLevel, null, warningMessage);

        global::log4net.Core.LoggingEvent[]? events = null;
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            events = _memoryAppender!.GetEvents();
            if (events.Any(e => e.Level == global::log4net.Core.Level.Warn &&
                                e.RenderedMessage.Contains(warningMessage)))
                break;

            await Task.Delay(50);
        }

        events = _memoryAppender!.GetEvents();

        events.Should().Contain(
            e => e.Level == global::log4net.Core.Level.Warn &&
                 e.RenderedMessage.Contains(warningMessage),
            "warning-level Akka events should be routed as log4net Warn events");
    }

    [Fact]
    public async Task AddLogger_Log4NetLogger_captures_error_level_events()
    {
        var actorSystem = _host!.Services.GetRequiredService<ActorSystem>();

        const string errorMessage = "This is an error from Akka via log4net hosting integration";

        var logger = Logging.GetLogger(actorSystem.EventStream, typeof(Log4NetHostingIntegrationSpecs).FullName!);
        logger.Log(AkkaLogLevel.ErrorLevel, null, errorMessage);

        global::log4net.Core.LoggingEvent[]? events = null;
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            events = _memoryAppender!.GetEvents();
            if (events.Any(e => e.Level == global::log4net.Core.Level.Error &&
                                e.RenderedMessage.Contains(errorMessage)))
                break;

            await Task.Delay(50);
        }

        events = _memoryAppender!.GetEvents();

        events.Should().Contain(
            e => e.Level == global::log4net.Core.Level.Error &&
                 e.RenderedMessage.Contains(errorMessage),
            "error-level Akka events should be routed as log4net Error events");
    }
}
