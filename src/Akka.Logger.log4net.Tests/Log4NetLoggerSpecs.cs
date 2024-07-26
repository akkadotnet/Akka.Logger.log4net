//-----------------------------------------------------------------------
// <copyright file="Log4NetLoggerSpecs.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Event;
using FluentAssertions;
using log4net.Core;
using log4net.Util;
using NSubstitute;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Logger.log4net.Tests;

public static class Log4NetLoggerSpecs
{
    public static readonly Exception Cause = new(nameof(Cause));

    public static readonly Exception? NoCause = null;

    private static LogEvent CreateLogEvent(Type logEventType, Exception? cause, LogSource logSource, object message)
        => (LogEvent)Activator.CreateInstance(logEventType, cause, logSource.Source, logSource.Type, message);

    private static Level GetLevelForLogEventType(Type logEventType)
        => logEventType switch
        {
            var type when type == typeof(Debug) => Level.Debug,
            var type when type == typeof(Info) => Level.Info,
            var type when type == typeof(Warning) => Level.Warn,
            var type when type == typeof(Error) => Level.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(logEventType)),
        };

    public class Method_CreateLoggingEvent(ITestOutputHelper output) : Log4NetSpecsBase(output)
    {
        private const string NA = Log4NetLogger.NA;

        public static readonly object?[][] Should_return_LoggingEvent_with_message_and_properties_MemberData =
        [
            [typeof(Debug), Cause, Properties.Create()
                .SetProperties(
                    declaringType: typeof(Method_CreateLoggingEvent))
                .AsReadOnly()],

            [typeof(Info), NoCause, Properties.Create()
                .SetProperties(
                    declaringType: typeof(Method_CreateLoggingEvent),
                    methodName: nameof(Should_return_LoggingEvent_with_message_and_properties))
                .AsReadOnly()],

            [typeof(Warning), Cause, Properties.Create()
                .SetProperties(
                    declaringType: typeof(Method_CreateLoggingEvent),
                    methodName: nameof(Should_return_LoggingEvent_with_message_and_properties),
                    fileName: "Log4NetLoggerSpec.cs")
                .AsReadOnly()],

            [typeof(Error), NoCause, Properties.Create()
                .SetProperties(
                    declaringType: typeof(Method_CreateLoggingEvent),
                    methodName: nameof(Should_return_LoggingEvent_with_message_and_properties),
                    fileName: "Log4NetLoggerSpec.cs",
                    lineNumber: 42)
                .AsReadOnly()],
        ];

        [Theory]
        [MemberData(nameof(Should_return_LoggingEvent_with_message_and_properties_MemberData))]
        public void Should_return_LoggingEvent_with_message_and_properties(Type logEventType, Exception? cause, ReadOnlyPropertiesDictionary contextProperties)
        {
            // Arrange

            var logger = Substitute.For<ILogger>();

            var level = GetLevelForLogEventType(logEventType);

            const string message = nameof(message);

            var log4NetPayload = new Log4NetPayload(message, contextProperties);

            var logEvent = CreateLogEvent(logEventType, cause, LogSource, log4NetPayload);

            var logEventSenderPath = TestActor.Path;

            // Act

            var loggingEvent = Log4NetLogger.CreateLoggingEvent(logger, level, logEvent, logEventSenderPath);

            // Assert

            loggingEvent.Level.Should().Be(level);
            loggingEvent.LoggerName.Should().Be(LogSource.Type.FullName);

            var eventData = loggingEvent.GetLoggingEventData();
            eventData.Should().NotBeNull();

            eventData.Message.Should().Be((string)log4NetPayload.Message);

            eventData.ThreadName.Should().Be(Thread.CurrentThread.ManagedThreadId.ToString(NumberFormatInfo.InvariantInfo));

            eventData.ExceptionString.Should().Be(cause?.ToString());

            eventData.LocationInfo.Should().NotBeNull();
            {
                var locationInfo = eventData.LocationInfo;

                locationInfo.ClassName.Should().Be(contextProperties.GetDeclaringTypeName() ?? NA);
                locationInfo.MethodName.Should().Be(contextProperties.GetMethodName() ?? NA);
                locationInfo.FileName.Should().Be(contextProperties.GetFileName() ?? NA);
                locationInfo.LineNumber.Should().Be(contextProperties.GetLineNumber() ?? NA);
            }

            eventData.Properties.Should().NotBeNull();
            {
                var properties = eventData.Properties;

                properties.GetActorPath().Should().Be(logEventSenderPath);
                properties.GetLogSource().Should().Be(logEvent.LogSource);
                properties.GetDeclaringTypeName().Should().Be(contextProperties.GetDeclaringTypeName());
                properties.GetMethodName().Should().Be(contextProperties.GetMethodName());
                properties.GetFileName().Should().Be(contextProperties.GetFileName());
                properties.GetLineNumber().Should().Be(contextProperties.GetLineNumber());
            }
        }
    }

    public class Receive_LogEvent : Log4NetSpecsBase
    {
        private readonly ILogger _logger;

        public Receive_LogEvent(ITestOutputHelper output) : base(output)
        {
            _logger = Substitute.For<ILogger>();

            Log4NetLoggerActor.UnderlyingActor.GetLogger = _ => _logger;
        }

        [Theory]
        [InlineData(typeof(Debug))]
        [InlineData(typeof(Info))]
        [InlineData(typeof(Warning))]
        [InlineData(typeof(Error))]
        public void Should_log_corresponding_LoggingEvent(Type logEventType)
        {
            // Arrange

            var log4NetPayload = new Log4NetPayload(
                message: nameof(Should_log_corresponding_LoggingEvent),
                properties: Properties.Create().AsReadOnly());

            var loggingEvents = new List<LoggingEvent>();
            _logger.Log(Arg.Do<LoggingEvent>(loggingEvents.Add));

            var logEvent = CreateLogEvent(logEventType, Cause, LogSource, log4NetPayload);

            // Act

            Log4NetLoggerActor.Receive(message: logEvent, sender: TestActor);

            // Assert

            var loggingEvent = loggingEvents.Should().ContainSingle().Subject;

            loggingEvent.Level.Should().Be(GetLevelForLogEventType(logEventType));

            loggingEvent.LoggerName.Should().Be(LogSource.Type.FullName);

            loggingEvent.GetLoggingEventData().Should().NotBeNull();
            {
                var eventData = loggingEvent.GetLoggingEventData();

                eventData.Message.Should().Be((string)log4NetPayload.Message);
                eventData.ExceptionString.Should().Be(Cause.ToString());
                eventData.LocationInfo.Should().NotBeNull();
                eventData.Properties.Should().NotBeNull();
            }
        }
    }
}
