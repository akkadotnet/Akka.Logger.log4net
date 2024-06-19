using System;
using Akka.Actor;
using Akka.Event;

namespace Akka.Logger.log4net
{
    public static class Log4NetLoggingAdapterExtensions
    {
        /// <summary>
        /// Creates a new logging adapter using the specified context's event stream.
        /// </summary>
        /// <param name="context">The context used to configure the logging adapter.</param>
        /// <returns>The newly created logging adapter.</returns>
        public static ILoggingAdapter GetLogger<T>(this IActorContext context)
            where T : class, ILoggingAdapter
        {
            var logSource = context.Self.ToString();
            var logClass = context.Props.Type;

            return new Log4NetLoggingAdapter(context.System.EventStream, logSource, logClass);
        }

        public static ILoggingAdapter GetLogger<T>(this ActorSystem system, object logSourceObj)
            where T : class, ILoggingAdapter
        {
            if (logSourceObj is null)
                throw new ArgumentNullException(nameof(logSourceObj));

            var logSource = LogSource.Create(logSourceObj, system);
            return new Log4NetLoggingAdapter(system.EventStream, logSource.Source, logSource.Type);
        }

        public static ILoggingAdapter GetLogger<T>(this ActorSystem system, string logSource, Type logType)
            where T : class, ILoggingAdapter
        {
            return new Log4NetLoggingAdapter(system.EventStream, logSource, logType);
        }
    }
}
