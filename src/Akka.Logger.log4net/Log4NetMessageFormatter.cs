using Akka.Event;

namespace Akka.Logger.log4net
{
    /// <inheritdoc />
    /// <summary>
    /// This class contains methods used to convert log4net templated messages
    /// into normal text messages.
    /// </summary>
    public class Log4NetMessageFormatter : ILogMessageFormatter
    {
        public static readonly Log4NetMessageFormatter Instance = new();

        /// <inheritdoc />
        public string Format(string format, params object[] args)
            => string.Format(format, args);

        /// <inheritdoc />
        public string Format(string format, IEnumerable<object> args)
            => Format(format, args.ToArray());
    }
}
