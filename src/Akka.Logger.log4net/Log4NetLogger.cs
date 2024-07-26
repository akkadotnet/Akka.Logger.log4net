//-----------------------------------------------------------------------
// <copyright file="Log4NetLogger.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Actor;
using Akka.Event;
using Akka.Dispatch;
using log4net;
using log4net.Core;
using System.Globalization;

namespace Akka.Logger.log4net
{
    /// <summary>
    /// This class is used to receive log events and sends them to
    /// the configured log4net logger. The following log events are
    /// recognized: <see cref="Debug"/>, <see cref="Info"/>,
    /// <see cref="Warning"/> and <see cref="Error"/>.
    /// </summary>
    public sealed class Log4NetLogger : ReceiveActor, IRequiresMessageQueue<ILoggerMessageQueueSemantics>
    {
        // Used when location information is not available.
        internal const string NA = "?";

        private readonly ILoggingAdapter _log = Logging.GetLogger(
            loggingBus: Context.System.EventStream,
            logSourceObj: nameof(Log4NetLogger));

        /// <summary>
        /// Gets or sets the function that will be used to get the logger.
        /// </summary>
        /// <remarks>
        /// This property is solely there to allow unit tests to set a different logger factory.
        /// </remarks>
        internal Func<LogEvent, ILogger> GetLogger { get; set; } = GetLoggerFromLogManager;

        private void Log(Level level, LogEvent logEvent)
        {
            var logger = GetLogger(logEvent);
            var logEventSenderPath = Context.Sender.Path;
            var loggingEvent = CreateLoggingEvent(logger, level, logEvent, logEventSenderPath);
            logger.Log(loggingEvent);
        }

        private static ILogger GetLoggerFromLogManager(LogEvent logEvent)
        {
#if NET472
            var logger = LogManager.GetLogger(logEvent.LogClass.FullName);
#else
            var logger = LogManager.GetLogger(logEvent.LogClass);
#endif
            return logger.Logger;
        }

        internal static LoggingEvent CreateLoggingEvent(
            ILogger logger, Level level, LogEvent logEvent, ActorPath logEventSenderPath)
        {
            var (message, properties) = logEvent.Message is Log4NetPayload log4NetPayload
                ? (log4NetPayload.Message, log4NetPayload.Properties)
                : (logEvent.Message, Log4NetPayload.Empty.Properties);

            var className = properties.GetDeclaringTypeName() ?? logEvent.LogClass.FullName;
            
            var methodName = properties.GetMethodName() ?? NA;
            
            var fileName = properties.GetFileName() ?? NA;
            
            var lineNumber = properties.GetLineNumber() ?? NA;

            var logginEventData = new LoggingEventData
            {
                Level = level,
                LoggerName = logEvent.LogClass.FullName,
                TimeStampUtc = logEvent.Timestamp,
                Message = message?.ToString(),
                ThreadName = logEvent.Thread.ManagedThreadId.ToString(NumberFormatInfo.InvariantInfo),
                ExceptionString = logEvent.Cause?.ToString(),
                LocationInfo = new(className, methodName, fileName, lineNumber),
                Properties = Properties.Create()
                    .SetProperties(actorPath: logEventSenderPath, logSource: logEvent.LogSource)
                    .SetProperties(properties.AsEnumerable()),
            };

            return new(
                callerStackBoundaryDeclaringType: logEvent.LogClass,
                repository: logger.Repository,
                data: logginEventData);
        }

        private void Handle(Error logEvent)
            => Log(Level.Error, logEvent);

        private void Handle(Warning logEvent)
            => Log(Level.Warn, logEvent);

        private void Handle(Info logEvent)
            => Log(Level.Info, logEvent);

        private void Handle(Debug logEvent)
            => Log(Level.Debug, logEvent);

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
        /// </summary>
        public Log4NetLogger()
        {
            Receive<Error>(Handle);
            Receive<Warning>(Handle);
            Receive<Info>(Handle);
            Receive<Debug>(Handle);
            Receive<InitializeLogger>(m =>
            {
                _log.Info("log4net started");
                Sender.Tell(new LoggerInitialized());
            });
        }
    }
}
