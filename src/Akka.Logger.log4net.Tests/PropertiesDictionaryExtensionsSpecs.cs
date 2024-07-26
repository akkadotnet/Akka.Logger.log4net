//-----------------------------------------------------------------------
// <copyright file="PropertiesDictionaryExtensionsSpecs.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using FluentAssertions;
using System.Collections;
using Xunit;

namespace Akka.Logger.log4net.Tests;

public static class PropertiesDictionaryExtensionsSpecs
{
    public class Method_SetProperties
    {
        [Fact]
        public void Should_set_properties()
        {
            var keyValuePairs = new[]
            {
                Properties.CreateProperty("key1", "value1"),
                Properties.CreateProperty("key2", "value2"),
            };

            var properties = Properties.Create().SetProperties(keyValuePairs);

            properties.Should().BeEquivalentTo(keyValuePairs);
        }

        [Fact]
        public void Should_overwrite_existing_properties()
        {
            var properties = Properties.Create()
                .SetProperties([Properties.CreateProperty("key", "value")]);

            var keyValuePairs = new[] { Properties.CreateProperty("key", "new value") };

            properties.SetProperties(keyValuePairs);

            properties.Should().BeEquivalentTo((IEnumerable)keyValuePairs);
        }
    }

    public class Method_AsEnumerable
    {
        [Fact]
        public void Should_return_properties_as_key_value_pairs()
        {
            var keyValuePairs = new[]
            {
                Properties.CreateProperty("key1", "value1"),
                Properties.CreateProperty("key2", "value2"),
            };

            var properties = Properties.Create()
                .SetProperties(keyValuePairs)
                .AsEnumerable();

            properties.Should().BeEquivalentTo(keyValuePairs);
        }
    }
}
