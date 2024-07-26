
# Akka.Logger.log4net

- This plugin integrates log4net with Akka.NET, and enhances your Akka.NET logging capabilities by integrating with log4net. For detailed usage and configuration examples, see the sections below or explore our [documentation](http://getakka.net/articles/utilities/logging.html).

- Currently targetting Log4Net 2.0.17

## Using the log4net Logger Integration

With log4net, you can enrich your logs with both custom and automatic context information, such as file names, line numbers, and method names. This makes your logs more informative and easier to trace.

### Obtaining an ILoggerAdapter with Contextual Logging

Use `Log4NetLoggingAdapterExtensions` to get an `ILoggerAdapter` that supports enriched logging:

#### Examples

##### Logging with Automatic Context Information

Simply calling `ForContext` without arguments adds useful context like file name, line number, and method name to your logs:

```csharp
var logger = Context.GetLogger().ForContext();

logger.Info("This log includes automatic context information.");
```

##### Logging with Custom Properties

Add custom properties to your logs as follows:

```csharp
var logger = Context.GetLogger()
    .ForContext("CustomProperty1", "CustomValue1")
    .ForContext("CustomProperty2", "CustomValue2");

logger.Info("This log has custom properties.");
```

Or, use a dictionary for multiple properties (an enumerable of key-value pairs):

```csharp
var properties = new Dictionary<string, object?>
{
    ["UserId"] = "user123",
    ["Operation"] = "UpdateProfile"
};

var logger = Context.GetLogger().ForContext(properties);
logger.Info("User profile updated.");
```

This will log a message with custom properties `UserId` and `Operation`, along with the source file name, line number, and method name.


### Configuring log4net for Custom Properties

To include custom properties in your logs, configure log4net like this:

```xml
<log4net>
  <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
	<layout type="log4net.Layout.PatternLayout">
	  <conversionPattern value="%date [%thread] [%file(%line)] [%property{LogSource} - %property{ActorPath}] %-5level %logger - UserId=%property{UserId}, Operation=%property{Operation} %class.%method - %message%newline" />
	</layout>
  </appender>
  <root>
	<level value="INFO" />
	<appender-ref ref="ConsoleAppender" />
  </root>
</log4net>
```

This setup ensures your logs include all the specified details, making them more informative and easier to navigate.

### Conclusion

The log4net integration enhances Akka.NET's logging by allowing for detailed contextual information in logs. This not only improves diagnostics but also aids in understanding the execution flow and context of log messages.

## Maintainer

- Akka.NET Team
