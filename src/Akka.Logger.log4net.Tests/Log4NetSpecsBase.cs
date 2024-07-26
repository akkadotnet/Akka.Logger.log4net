//-----------------------------------------------------------------------
// <copyright file="Log4NetSpecsBase.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Configuration;
using Akka.Event;
using Akka.TestKit;
using Xunit.Abstractions;

namespace Akka.Logger.log4net.Tests;

public abstract class Log4NetSpecsBase : TestKit.Xunit2.TestKit
{
    public static readonly Config Config =
        """
            akka.loglevel = DEBUG
            akka.loggers=["Akka.Logger.log4net.Log4NetLogger, Akka.Logger.log4net"]
            """;

    private readonly LogSource _logSource;
    private readonly Lazy<Log4NetLoggingAdapter> _loggingAdapter;
    private readonly Lazy<TestActorRef<Log4NetLogger>> _log4NetLoggerActor;

    protected Log4NetSpecsBase(ITestOutputHelper output)
        : base(Config, output: output)
    {
        _logSource = LogSource.Create(TestActor);
        _loggingAdapter = new(CreateLoggingAdapter);
        _log4NetLoggerActor = new(CreateLog4NetLogger);
    }

    protected LogSource LogSource => _logSource;

    protected Log4NetLoggingAdapter Log4NetLoggingAdapter => _loggingAdapter.Value;

    protected TestActorRef<Log4NetLogger> Log4NetLoggerActor => _log4NetLoggerActor.Value;

    private Log4NetLoggingAdapter CreateLoggingAdapter()
        => (Log4NetLoggingAdapter)Sys.GetLogger(LogSource.Source, LogSource.Type);

    private TestActorRef<Log4NetLogger> CreateLog4NetLogger()
        => ActorOfAsTestActorRef(() => new Log4NetLogger(), name: nameof(Log4NetLogger));
}
