//-----------------------------------------------------------------------
// <copyright file="Properties.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET Project <https://github.com/AkkaNetContrib>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Actor;
using log4net.Util;
using System.Runtime.CompilerServices;

namespace Akka.Logger.log4net
{
    /// <summary>
    /// This class contains the property names used to enrich log events.
    /// </summary>
    public static class Properties
    {
        public static readonly IReadOnlyCollection<string> CallerInfoProperties =
        [
            DeclaringTypeName,
            FileName,
            LineNumber,
            MethodName
        ];

        /// <summary>
        /// Property name for the actor path of the actor which created the log event.
        /// </summary>
        public const string ActorPath = nameof(ActorPath);

        /// <summary>
        /// Property name for the class name of the method which created the log event.
        /// </summary>
        public const string DeclaringTypeName = nameof(DeclaringTypeName);

        /// <summary>
        /// Property name for the file name of the source file from where the log event was created.
        /// </summary>
        public const string FileName = nameof(FileName);

        /// <summary>
        /// Property name for the source line number in the source file from where the log event was created.
        /// </summary>
        public const string LineNumber = nameof(LineNumber);

        /// <summary>
        /// Property name for the log source of the log event.
        /// </summary>
        public const string LogSource = nameof(LogSource);

        /// <summary>
        /// Property name for the method name of the method which created the log event.
        /// </summary>
        public const string MethodName = nameof(MethodName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static KeyValuePair<string, object?> CreateProperty(string propertyName, object? value)
            => new(propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PropertiesDictionary Create()
            => new();
    }
}
