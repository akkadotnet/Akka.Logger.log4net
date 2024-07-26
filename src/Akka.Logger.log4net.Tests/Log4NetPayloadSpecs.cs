//-----------------------------------------------------------------------
// <copyright file="Log4NetPayloadSpecs.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using FluentAssertions;
using log4net.Util;
using Xunit;

namespace Akka.Logger.log4net.Tests;

public static class Log4NetPayloadSpecs
{
    public class Ctor
    {
        [Fact]
        public void Should_create_instance_with_message_and_properties()
        {
            const string message = nameof(message);
            
            var properties = Properties.Create()
                .SetProperties([Properties.CreateProperty("key", "value")])
                .AsReadOnly();

            var payload = new Log4NetPayload(message, properties);
            payload.Message.Should().Be(message);
            payload.Properties.Should().BeEquivalentTo(properties);
        }

        [Fact]
        public void Should_throw_ArgumentNullException_When_message_is_null()
        {
            Action action = () => _ = new Log4NetPayload(message: null!, properties: []);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("message");
        }

        [Fact]
        public void Should_have_empty_properties_When_properties_is_null()
        {
            var payload = new Log4NetPayload(message: "message", properties: null!);
            payload.Properties.Should().NotBeNull();
            payload.Properties.Should().BeEquivalentTo<ReadOnlyPropertiesDictionary>([]);
        }
    }

    public class Field_Empty
    {
        [Fact]
        public void Should_be_correctly_initialized()
        {
            var empty = Log4NetPayload.Empty;
            empty.Message.Should().NotBeNull();
            empty.Properties.Should().NotBeNull();
            empty.Properties.Should().BeEquivalentTo<ReadOnlyPropertiesDictionary>([]);
        }
    }

    public class Method_ToString
    {
        [Fact]
        public void  Should_return_Message_from_ToString()
        {
            const string message = nameof(message);
            var payload = new Log4NetPayload(message, properties: []);
            payload.ToString().Should().Be(message);
        }
    }
}
