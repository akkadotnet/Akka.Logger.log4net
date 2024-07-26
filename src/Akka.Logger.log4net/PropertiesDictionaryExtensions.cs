//-----------------------------------------------------------------------
// <copyright file="PropertiesDictionaryExtensions.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Actor;
using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Akka.Logger.log4net
{
    /// <summary>
    /// Provides extension methods for <see cref="PropertiesDictionary"/>.
    /// </summary>
    public static class PropertiesDictionaryExtensions
    {
        internal static PropertiesDictionary SetProperties(
            this PropertiesDictionary properties,
            Type? declaringType = null,
            string? methodName = null,
            string? fileName = null,
            int lineNumber = 0,
            ActorPath? actorPath = null,
            string? logSource = null)
        {
            if (actorPath is not null)
                properties.SetActorPath(actorPath);

            if (lineNumber > 0)
                properties.SetLineNumber(lineNumber);

            if (declaringType is not null)
                properties.SetDeclaringTypeName(declaringType);

            if (methodName is not null)
                properties.SetMethodName(methodName);

            if (fileName is not null)
                properties.SetFileName(fileName);

            if (logSource is not null)
                properties.SetLogSource(logSource);

            return properties;
        }
        
        /// <summary>
        /// Add a range of key-value pairs to the <see cref="PropertiesDictionary"/>.
        /// </summary>
        /// <param name="properties">The properties to modify.</param>
        /// <param name="keyValuePairs">The properties to add (or update).</param>
        internal static PropertiesDictionary SetProperties(this PropertiesDictionary properties, IEnumerable<KeyValuePair<string, object?>> keyValuePairs)
        {
            foreach (var pair in keyValuePairs)
                properties[pair.Key] = pair.Value;

            return properties;
        }

        /// <summary>
        /// Return the <see cref="PropertiesDictionary"/> as an enumerable of key-value pairs.
        /// </summary>
        /// <param name="properties">The properties which should by returned as key-value pairs.</param>
        /// <returns>The properties as key-value pairs.</returns>
        internal static IEnumerable<KeyValuePair<string, object?>> AsEnumerable(this ReadOnlyPropertiesDictionary properties)
            => properties
                .Cast<DictionaryEntry>()
                .Select(property => Properties.CreateProperty((string)property.Key, property.Value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlyPropertiesDictionary AsReadOnly(this PropertiesDictionary properties)
            => properties;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ActorPath? GetActorPath(this ReadOnlyPropertiesDictionary properties)
            => (ActorPath?)properties[Properties.ActorPath];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetActorPath(this PropertiesDictionary properties, ActorPath actorPath)
            => properties[Properties.ActorPath] = actorPath;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? GetLogSource(this ReadOnlyPropertiesDictionary properties)
            => (string?)properties[Properties.LogSource];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetLogSource(this PropertiesDictionary properties, string logSource)
            => properties[Properties.LogSource] = logSource;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? GetDeclaringTypeName(this ReadOnlyPropertiesDictionary properties)
            => (string?)properties[Properties.DeclaringTypeName];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetDeclaringTypeName(this PropertiesDictionary properties, Type declaringType)
            => properties[Properties.DeclaringTypeName] = declaringType.FullName;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? GetMethodName(this ReadOnlyPropertiesDictionary properties)
            => (string?)properties[Properties.MethodName];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetMethodName(this PropertiesDictionary properties, string methodName)
            => properties[Properties.MethodName] = methodName;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? GetFileName(this ReadOnlyPropertiesDictionary properties)
            => (string?)properties[Properties.FileName];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetFileName(this PropertiesDictionary properties, string fileName)
            => properties[Properties.FileName] = fileName;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? GetLineNumber(this ReadOnlyPropertiesDictionary properties)
            => properties[Properties.LineNumber] is int lineNumber
                ? lineNumber.ToString(NumberFormatInfo.InvariantInfo)
                : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetLineNumber(this PropertiesDictionary properties, int lineNumber)
            => properties[Properties.LineNumber] = lineNumber;
    }
}
