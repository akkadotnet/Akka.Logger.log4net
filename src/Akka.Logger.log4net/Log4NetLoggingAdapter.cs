using Akka.Event;

namespace Akka.Logger.log4net
{
    public class Log4NetLoggingAdapter : LoggingAdapterBase
    {
        private readonly LoggingBus _bus;
        private readonly Type _logClass;
        private readonly string _logSource;

        /// <inheritdoc />
        public Log4NetLoggingAdapter(LoggingBus bus, string logSource, Type logClass)
            : base(Log4NetMessageFormatter.Instance)
        {
            _bus = bus;
            _logSource = logSource;
            _logClass = logClass;

            IsErrorEnabled = bus.LogLevel <= LogLevel.ErrorLevel;
            IsWarningEnabled = bus.LogLevel <= LogLevel.WarningLevel;
            IsInfoEnabled = bus.LogLevel <= LogLevel.InfoLevel;
            IsDebugEnabled = bus.LogLevel <= LogLevel.DebugLevel;
        }

        private LogEvent CreateLogEvent(LogLevel logLevel, object message, Exception? cause = null)
            => logLevel switch
            {
                LogLevel.DebugLevel => new Debug(cause, _logSource, _logClass, message),
                LogLevel.InfoLevel => new Info(cause, _logSource, _logClass, message),
                LogLevel.WarningLevel => new Warning(cause, _logSource, _logClass, message),
                LogLevel.ErrorLevel => new Error(cause, _logSource, _logClass, message),
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, message: null)
            };

        /// <inheritdoc />
        protected override void NotifyLog(LogLevel logLevel, object message, Exception? cause = null)
            => _bus.Publish(CreateLogEvent(logLevel, message, cause));

        /// <inheritdoc />
        public override bool IsDebugEnabled { get; }

        /// <inheritdoc />
        public override bool IsInfoEnabled { get; }

        /// <inheritdoc />
        public override bool IsWarningEnabled { get; }

        /// <inheritdoc />
        public override bool IsErrorEnabled { get; }
    }
}
