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
using System.Runtime.CompilerServices;

namespace Akka.Logger.log4net
{
    /// <summary>
    /// This class is used to receive log events and sends them to
    /// the configured log4net logger. The following log events are
    /// recognized: <see cref="Debug"/>, <see cref="Info"/>,
    /// <see cref="Warning"/> and <see cref="Error"/>.
    /// </summary>
    public class Log4NetLogger : ReceiveActor, IRequiresMessageQueue<ILoggerMessageQueueSemantics>
    {
        private readonly ILoggingAdapter _log = Logging.GetLogger(Context.System.EventStream, "Log4NetLogger");

        private static void Log(LogEvent logEvent, Action<ILog> logStatement)
        {
#if NET472
            var logger = LogManager.GetLogger(logEvent.LogClass.FullName);
#else
            var logger = LogManager.GetLogger(logEvent.LogClass);
#endif
            logStatement(logger);
        }

        private static void Handle(Error logEvent)
            => Log(logEvent, logger => logger.ErrorFormat("{0}", logEvent.Message));

        private static void Handle(Warning logEvent)
            => Log(logEvent, logger => logger.WarnFormat("{0}", logEvent.Message));

        private static void Handle(Info logEvent)
            => Log(logEvent, logger => logger.InfoFormat("{0}", logEvent.Message));

        private static void Handle(Debug logEvent)
            => Log(logEvent, logger => logger.DebugFormat("{0}", logEvent.Message));

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
