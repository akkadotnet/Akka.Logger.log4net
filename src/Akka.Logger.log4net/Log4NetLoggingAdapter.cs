//-----------------------------------------------------------------------
// <copyright file="Log4NetLoggingAdapter.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Event;
using log4net.Util;
using System.Diagnostics;
using System.Reflection;

namespace Akka.Logger.log4net
{
    using Debug = Event.Debug;

    /// <summary>
    /// The logging adapter used for creating <see cref="LogEvent"/> instances and publishing them
    /// to the <see cref="LoggingBus"/>.
    /// </summary>
    public sealed class Log4NetLoggingAdapter : LoggingAdapterBase
    {
        private readonly LoggingBus _bus;
        private readonly LogSource _logSource;
        private readonly PropertyNode _propertyNodeListHead;

        public Log4NetLoggingAdapter(LoggingBus bus, LogSource logSource)
            : this(bus, logSource, PropertyNode.Empty)
        {
        }

        private Log4NetLoggingAdapter(LoggingBus bus, LogSource logSource, PropertyNode propertyNode)
            : base(Log4NetMessageFormatter.Instance)
        {
            _bus = bus;
            _logSource = logSource;
            _propertyNodeListHead = propertyNode;

            IsErrorEnabled = bus.LogLevel <= LogLevel.ErrorLevel;
            IsWarningEnabled = bus.LogLevel <= LogLevel.WarningLevel;
            IsInfoEnabled = bus.LogLevel <= LogLevel.InfoLevel;
            IsDebugEnabled = bus.LogLevel <= LogLevel.DebugLevel;
        }

        internal LogEvent CreateLogEvent(LogLevel logLevel, object message, Exception? cause = null)
            => logLevel switch
            {
                LogLevel.DebugLevel => new Debug(cause, _logSource.Source, _logSource.Type, BuildMessage(message)),
                LogLevel.InfoLevel => new Info(cause, _logSource.Source, _logSource.Type, BuildMessage(message)),
                LogLevel.WarningLevel => new Warning(cause, _logSource.Source, _logSource.Type, BuildMessage(message)),
                LogLevel.ErrorLevel => new Error(cause, _logSource.Source, _logSource.Type, BuildMessage(message)),
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

        /// <summary>
        /// Get the configured context properties.
        /// </summary>
        /// <returns>The context proeprties.</returns>
        public PropertiesDictionary GetContextProperties()
            => Properties.Create().SetProperties(_propertyNodeListHead.GetProperties());

        /// <summary>
        /// Set a context property for the logger.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <returns>A new instance of this class which has the provided property set.</returns>
        public Log4NetLoggingAdapter SetContextProperty(string propertyName, object? value)
        {
            var property = Properties.CreateProperty(propertyName, value);
            var propertyNode = _propertyNodeListHead.Add(property);
            return new Log4NetLoggingAdapter(_bus, _logSource, propertyNode);
        }

        /// <summary>
        /// Set a range of context properties for the logger.
        /// </summary>
        /// <param name="properties">The properties to set.</param>
        /// <returns>A new instance of this class which has the provided properties set.</returns>
        public Log4NetLoggingAdapter SetContextProperties(IEnumerable<KeyValuePair<string, object?>> properties)
        {
            properties ??= [];

            var propertyNode = _propertyNodeListHead.AddRange(properties);
            return ReferenceEquals(propertyNode, _propertyNodeListHead)
                ? this
                : new Log4NetLoggingAdapter(_bus, _logSource, propertyNode);
        }

        /// <summary>
        /// Create a new <see cref="Log4NetPayload"/> with the provided <paramref name="message"/>
        /// and all the properties in the linked list represented by the <see cref="_propertyNodeListHead"/>.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <returns>The corresponding <see cref="Log4NetPayload"/>.</returns>
        internal Log4NetPayload BuildMessage(object message)
        {
            var properties = GetContextProperties();
            AddCallerInfoFromStackTraceIfMissing(properties);
            return new(message, properties);
        }

        /// <summary>
        /// Only consult the stack trace for adding missing caller info if it is not already present.
        /// </summary>
        /// <remarks>
        /// This is to avoid the overhead of stack trace generation if it is not needed.
        /// </remarks>
        /// <param name="properties">The properties to be supplemented with caller info.</param>
        internal void AddCallerInfoFromStackTraceIfMissing(PropertiesDictionary properties)
        {
            // If complete caller info is already present then there is no need to add it again
            // (and we can avoid to create a costly stack trace).
            if (Properties.CallerInfoProperties.All(properties.Contains))
                return;

            // If the caller stack frame cannot be found then we cannot add caller info.
            if (GetCallerStackFrame(_logSource.Type) is not { } frame)
                return;
            
            if (frame.GetMethod() is { } method)
            {
                properties.SetDeclaringTypeName(method.DeclaringType);
                properties.SetMethodName(method.Name);
            }

            if (frame.GetFileLineNumber() is { } lineNumber and > 0)
                properties.SetLineNumber(lineNumber);

            if (frame.GetFileName() is { } fileName)
                properties.SetFileName(fileName);

            #region Helper function(s)

            // Get the stack frame of the caller of the method in the provided 'logClass' from
            // the stack trace.
            // (Note that generating a stack trace is costly and should be avoided if possible.)
            static StackFrame? GetCallerStackFrame(Type logClass)
            {
                // Get the sequence of stack frames from the 'StackTrace' object.
                if (new StackTrace(fNeedFileInfo: true).GetFrames() is not { } frames)
                    return null;

                // Try to find the frame where the declaring type of the frame's method is the
                // provided 'logClass'.
                foreach (var frame in frames)
                {
                    // Traverse the declaring types of the method up to the root type and try to
                    // find the 'logClass'.
                    var type = frame.GetMethod()?.DeclaringType;
                    while (type is not null)
                    {
                        // If 'true' we have found the caller.
                        if (type == logClass)
                            return frame;

                        type = type.DeclaringType;
                    }
                }

                return null;
            }

            #endregion
        }

        /// <summary>
        /// Represents a property in a single-linked list of properties.
        /// </summary>
        private sealed record PropertyNode
        {
            /// <summary>
            /// Represents an empty instance of this class.
            /// </summary>
            public static readonly PropertyNode Empty = new(
                property: Properties.CreateProperty(string.Empty, null),
                next: null!);

            /// <summary>
            /// The property represented as key-value pair.
            /// </summary>
            public KeyValuePair<string, object?> Property { get; }

            /// <summary>
            /// The next property node in the linked list.
            /// </summary>
            public PropertyNode Next { get; }

            /// <summary>
            /// Create a new instance of this class which will be the head of a linked
            /// list of properties.
            /// </summary>
            /// <param name="property">The property to add.</param>
            /// <param name="next">The old head of the linked list.</param>
            public PropertyNode(KeyValuePair<string, object?> property, PropertyNode next)
            {
                this.Property = property;
                this.Next = next;
            }

            /// <summary>
            /// Add a new property to the head of the linked list (by returning a new <see cref="PropertyNode"/>
            /// as the new head of the linked list).
            /// </summary>
            /// <param propertyName="propertyName">The property propertyName (must not be <c>null</c>.</param>
            /// <param propertyName="value">The property value.</param>
            /// <returns>A new <see cref="PropertyNode"/> as the new head of the linked list.</returns>
            public PropertyNode Add(KeyValuePair<string, object?> property)
                => new(property, this);

            /// <summary>
            /// Add a range of properties to the head of the linked list.
            /// </summary>
            /// <param propertyName="properties">The properties to add.</param>
            /// <returns>A new <see cref="PropertyNode"/> as the new head of the linked list.</returns>
            public PropertyNode AddRange(IEnumerable<KeyValuePair<string, object?>> properties)
            {
                var currentNode = this;
                foreach (var property in properties)
                    currentNode = currentNode.Add(property);

                return currentNode;
            }

            /// <summary>
            /// Return all properties stored in the linked list.
            /// </summary>
            /// <returns>The properties stored in the linked list.</returns>
            public IEnumerable<KeyValuePair<string, object?>> GetProperties()
            {
                var currentNode = this;
                while (currentNode != Empty)
                {
                    yield return currentNode!.Property;
                    currentNode = currentNode.Next;
                }
            }
        }
    }
}
