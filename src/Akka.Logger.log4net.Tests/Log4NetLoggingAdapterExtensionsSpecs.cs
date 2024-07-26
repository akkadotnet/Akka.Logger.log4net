//-----------------------------------------------------------------------
// <copyright file="Log4NetLoggingAdapterExtensionsSpecs.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Logger.log4net.Tests;

public static class Log4NetLoggingAdapterExtensionsSpecs
{
    public class Method_ForContext(ITestOutputHelper output) : Log4NetSpecsBase(output)
    {
        [Fact]
        public void Should_throw_ArgumentNullException_When_adapter_is_null()
        {
            Action action = () => Log4NetLoggingAdapterExtensions.ForContext(
                adapter: null!,
                properties: new Dictionary<string, object?>());

            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("adapter");
        }

        [Fact]
        public void Should_return_adapter_When_properties_and_real_world_context_is_null()
        {
            var loggingAdapter = Log4NetLoggingAdapter.ForContext(
                properties: null, fileName: null, lineNumber: 0, methodName: null);

            loggingAdapter.Should().BeSameAs(Log4NetLoggingAdapter);
        }

        [Fact]
        public void Should_return_adapter_with_real_world_context()
        {
            var loggingAdapter = Log4NetLoggingAdapter.ForContext();

            loggingAdapter.Should().NotBeNull();
            loggingAdapter.Should().NotBeSameAs(Log4NetLoggingAdapter);
            
            var log4NetLoggingAdapter = loggingAdapter.Should().BeOfType<Log4NetLoggingAdapter>().Subject;
            
            var contextProperties = log4NetLoggingAdapter.GetContextProperties().AsEnumerable().ToArray();
            
            contextProperties.Should().ContainSingle(p => p.Key == Properties.FileName)
                .Which.Value.Should().NotBeNull();
            
            contextProperties.Should().ContainSingle(p => p.Key == Properties.LineNumber)
                .Which.Value.Should().NotBeNull();
            
            contextProperties.Should().ContainSingle(p => p.Key == Properties.MethodName);
        }

        [Fact]
        public void Should_return_adapter_with_real_world_context_and_properties_added()
        {
            var properties = new Dictionary<string, object?>
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            };

            var loggingAdapter = Log4NetLoggingAdapter.ForContext(
                properties: properties);

            loggingAdapter.Should().NotBeNull();
            loggingAdapter.Should().NotBeSameAs(Log4NetLoggingAdapter);
            var log4NetLoggingAdapter = loggingAdapter.Should().BeOfType<Log4NetLoggingAdapter>().Subject;

            var contextProperties = log4NetLoggingAdapter.GetContextProperties().AsEnumerable().ToArray();

            contextProperties.Should().ContainSingle(p => p.Key == Properties.FileName)
                .Which.Value.Should().NotBeNull();

            contextProperties.Should().ContainSingle(p => p.Key == Properties.LineNumber)
                .Which.Value.Should().NotBeNull();

            contextProperties.Should().ContainSingle(p => p.Key == Properties.MethodName);
        }

        [Fact]
        public void Should_return_adapter_with_real_world_context_and_property_added()
        {
            var property = Properties.CreateProperty("key", "value");
            var loggingAdapter = Log4NetLoggingAdapter.ForContext(propertyName: property.Key, value: property.Value);

            loggingAdapter.Should().NotBeNull();
            loggingAdapter.Should().NotBeSameAs(Log4NetLoggingAdapter);

            var log4NetLoggingAdapter = loggingAdapter.Should().BeOfType<Log4NetLoggingAdapter>().Subject;

            var contextProperties = log4NetLoggingAdapter.GetContextProperties().AsEnumerable().ToArray();

            contextProperties.Should().ContainSingle(p => p.Key == Properties.FileName)
                .Which.Value.Should().NotBeNull();

            contextProperties.Should().ContainSingle(p => p.Key == Properties.LineNumber)
                .Which.Value.Should().NotBeNull();

            contextProperties.Should().ContainSingle(p => p.Key == Properties.MethodName);
        }
    }
}
