//-----------------------------------------------------------------------
// <copyright file="Log4NetLoggingAdapterExtensions.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Actor;
using Akka.Event;
using System.Runtime.CompilerServices;

namespace Akka.Logger.log4net
{
    /// <summary>
    /// Provides extension methods for <see cref="ILoggingAdapter"/>.
    /// </summary>
    public static class Log4NetLoggingAdapterExtensions
    {
        /// <summary>
        /// Create a logger that enriches log events with the specified property.
        /// </summary>
        /// <remarks>
        /// The method parameters <paramref name="fileName"/>, <paramref name="lineNumber"/>
        /// and <paramref name="methodName"/> will be default initialized with "real-world"
        /// context information. So, you might want to let them untouched.
        /// </remarks>
        /// <param name="adapter">ILoggingAdapter instance</param>
        /// <param name="propertyName">The name of the property. Must be non-empty.</param>
        /// <param name="value">The property value.</param>
        /// <param name="fileName">The file name to log.</param>
        /// <param name="lineNumber">The line number to log.</param>
        /// <param name="methodName">The method name to log.</param>
        public static ILoggingAdapter ForContext(
            this ILoggingAdapter adapter,
            string propertyName,
            object? value,
            [CallerFilePath] string? fileName = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string? methodName = null)
        {
            return ForContext(
                adapter: adapter,
                properties: [Properties.CreateProperty(propertyName, value)],
                fileName: fileName,
                lineNumber: lineNumber,
                methodName: methodName);
        }

        /// <summary>
        /// Create a logger that enriches log events with the specified property.
        /// </summary>
        /// <remarks>
        /// The method parameters <paramref name="fileName"/>, <paramref name="lineNumber"/>
        /// and <paramref name="methodName"/> will be default initialized with "real-world"
        /// context information. So, you might want to let them untouched.
        /// </remarks>
        /// <param name="adapter">ILoggingAdapter instance</param>
        /// <param name="properties">The properties to log.</param>
        /// <param name="fileName">The file name to log.</param>
        /// <param name="lineNumber">The line number to log.</param>
        /// <param name="methodName">The method name to log.</param>
        public static ILoggingAdapter ForContext(
            this ILoggingAdapter adapter,
            IEnumerable<KeyValuePair<string, object?>>? properties = null,
            [CallerFilePath] string? fileName = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string? methodName = null)
        {
            if (adapter is null)
                throw new ArgumentNullException(nameof(adapter));

            // Log a warning if for the provided 'adapter' no 'Log4NetLoggingAdapter' can be obtained.
            if (Log4NetLoggingAdapterFor(adapter, out var errorMessage) is not { } enrichedAdapter)
            {
                adapter.Warning(errorMessage!);
                return adapter;
            }

            if (fileName is not null)
                enrichedAdapter = enrichedAdapter.SetContextProperty(Properties.FileName, fileName);

            if (lineNumber > 0)
                enrichedAdapter = enrichedAdapter.SetContextProperty(Properties.LineNumber, lineNumber);

            if (methodName is not null)
                enrichedAdapter = enrichedAdapter.SetContextProperty(Properties.MethodName, methodName);

            if (properties is not null)
                enrichedAdapter = enrichedAdapter.SetContextProperties(properties);

            return enrichedAdapter;

            #region Helper functions(s)

            // Try to get a 'Log4NetLoggingAdapter' from the provided 'adapter'.
            static Log4NetLoggingAdapter? Log4NetLoggingAdapterFor(ILoggingAdapter adapter, out string? errorMessage)
            {
                switch (adapter)
                {
                    case Log4NetLoggingAdapter log4NetLoggingAdapter:
                        errorMessage = null;
                        return log4NetLoggingAdapter;

                    case BusLogging defaultAkkaAdapter:
                        errorMessage = null;
                        return new Log4NetLoggingAdapter(
                            defaultAkkaAdapter.Bus,
                            LogSource.Create(defaultAkkaAdapter.LogSource, defaultAkkaAdapter.LogClass));

                    default:
                        errorMessage = $"Cannot enrich log event with properties because the adapter is not a {typeof(Log4NetLoggingAdapter)} or {typeof(BusLogging)}.";
                        return null;
                }
            }

            #endregion
        }

        /// <summary>
        /// Creates a new logging adapter using the specified context's event stream.
        /// </summary>
        /// <param name="context">The context used to configure the logging adapter.</param>
        /// <returns>The newly created logging adapter.</returns>
        public static ILoggingAdapter GetLogger(this IActorContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            return new Log4NetLoggingAdapter(context.System.EventStream, LogSource.Create(context));
        }

        public static ILoggingAdapter GetLogger(this ActorSystem system, object logSourceObj)
        {
            if (system is null)
                throw new ArgumentNullException(nameof(system));

            if (logSourceObj is null)
                throw new ArgumentNullException(nameof(logSourceObj));

            return new Log4NetLoggingAdapter(system.EventStream, LogSource.Create(logSourceObj, system));
        }

        public static ILoggingAdapter GetLogger(this ActorSystem system, string logSource, Type logType)
        {
            if (system is null)
                throw new ArgumentNullException(nameof(system));

            if (logSource is null)
                throw new ArgumentNullException(nameof(logSource));

            return new Log4NetLoggingAdapter(system.EventStream, LogSource.Create(logSource, logType));
        }
    }
}
