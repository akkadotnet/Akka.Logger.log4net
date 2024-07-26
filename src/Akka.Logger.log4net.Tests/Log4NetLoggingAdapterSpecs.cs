//-----------------------------------------------------------------------
// <copyright file="Log4NetLoggingAdapterSpecs.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Event;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Logger.log4net.Tests;

public static class Log4NetLoggingAdapterSpecs
{
    public class Method_BuildMessage(ITestOutputHelper output) : Log4NetSpecsBase(output)
    {
        [Fact]
        public void Should_return_payload_with_message_and_properties()
        {
            const string message = "message";

            var logginAdapter = Log4NetLoggingAdapter.SetContextProperties(new Dictionary<string, object?>
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            });

            Log4NetPayload payload = logginAdapter.BuildMessage(message);

            payload.Message.Should().Be(message);
            payload.Properties.Should().BeEquivalentTo(logginAdapter.GetContextProperties());
        }
    }

    public class Method_CreateLogEvent(ITestOutputHelper output) : Log4NetSpecsBase(output)
    {
        [Theory]
        [InlineData(LogLevel.DebugLevel, typeof(Debug), true)]
        [InlineData(LogLevel.DebugLevel, typeof(Debug), false)]
        [InlineData(LogLevel.InfoLevel, typeof(Info), true)]
        [InlineData(LogLevel.InfoLevel, typeof(Info), false)]
        [InlineData(LogLevel.WarningLevel, typeof(Warning), true)]
        [InlineData(LogLevel.WarningLevel, typeof(Warning), false)]
        [InlineData(LogLevel.ErrorLevel, typeof(Error), true)]
        [InlineData(LogLevel.ErrorLevel, typeof(Error), false)]
        public void Should_return_LogEvent(LogLevel logLevel, Type logEventType, bool withCause)
        {
            const string message = "message";

            var cause = withCause
                ? new Exception("cause")
                : null;

            LogEvent logEvent = Log4NetLoggingAdapter.CreateLogEvent(logLevel, message: message, cause: cause);

            logEvent.GetType().Should().Be(logEventType);
            logEvent.LogLevel().Should().Be(logLevel);

            var expectedPayload = Log4NetLoggingAdapter.BuildMessage(message);
            var payload = logEvent.Message.Should().BeOfType<Log4NetPayload>().Subject;
            payload.Message.Should().Be(expectedPayload.Message);
            payload.Properties.Should().BeEquivalentTo(expectedPayload.Properties);
            logEvent.Cause.Should().BeSameAs(cause);
        }
    }

    public class Method_SetContextProperties(ITestOutputHelper output) : Log4NetSpecsBase(output)
    {
        [Fact]
        public void Should_return_adapter_When_properties_is_null()
        {
            var loggingAdapter = Log4NetLoggingAdapter.SetContextProperties(properties: null!);
            loggingAdapter.Should().BeSameAs(Log4NetLoggingAdapter);
        }

        [Fact]
        public void Should_return_adapter_When_properties_is_empty()
        {
            var loggingAdapter = Log4NetLoggingAdapter.SetContextProperties(properties: []);
            loggingAdapter.Should().BeSameAs(Log4NetLoggingAdapter);
        }

        [Fact]
        public void Should_return_new_adapter_with_properties_added_When_properties_is_not_empty()
        {
            IEnumerable<KeyValuePair<string, object?>> properties = new Dictionary<string, object?>
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            };

            var loggingAdapter = Log4NetLoggingAdapter.SetContextProperties(properties);
            
            loggingAdapter.Should().NotBeNull();
            loggingAdapter.Should().NotBeSameAs(Log4NetLoggingAdapter);
            loggingAdapter.GetContextProperties().AsEnumerable().Should().BeEquivalentTo(properties);
        }

        [Fact]
        public void Should_return_new_adapter_with_property_added()
        {
            var property = Properties.CreateProperty("key1", "value1");

            var loggingAdapter = Log4NetLoggingAdapter.SetContextProperty(propertyName: property.Key, value: property.Value);

            loggingAdapter.Should().NotBeNull();
            loggingAdapter.Should().NotBeSameAs(Log4NetLoggingAdapter);
            loggingAdapter.GetContextProperties().AsEnumerable().Should().BeEquivalentTo([property]);
        }
    }
}
