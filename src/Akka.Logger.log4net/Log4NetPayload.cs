//-----------------------------------------------------------------------
// <copyright file="Log4NetPayload.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using log4net.Util;

namespace Akka.Logger.log4net
{
    /// <summary>
    /// Will be sent by the <see cref="Log4NetLoggingAdapter"/> as a log message. It wraps the
    /// original log message and the propsDict to be logged.
    /// </summary>
    internal readonly record struct Log4NetPayload
    {
        /// <summary>
        /// An empty instance of this class.
        /// </summary>
        public static readonly Log4NetPayload Empty = new(message: new object(), properties: []);

        /// <summary>
        /// The message to be logged.
        /// </summary>
        public object Message { get; } = Empty.Message;

        /// <summary>
        /// The propsDict to be logged.
        /// </summary>
        public ReadOnlyPropertiesDictionary Properties { get; } = Empty.Properties;

        /// <summary>
        /// Create an instance of this class with the specified message and properties.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="properties">The propsDict to be logged.</param>
        public Log4NetPayload(object message, ReadOnlyPropertiesDictionary properties)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Properties = properties ?? Empty.Properties;
        }

        /// <inheritdoc />
        public override string ToString()
            => Message.ToString();
    }
}
