![Stopwatch Manager](/images/stopwatch-manager.png)
# Stopwatch Manager for .NET

Have you ever had to record timings for a multi-threaded application and realized you needed
to record the length of so many events that you actually need a collection of
stopwatches instead of a single stopwatch? Yeah, me too.

That's why I developed the Stopwatch Manager for .NET.  The package supports .NET 6 & 7, and
it creates a concurrent dictionary of stopwatches, each with its own unique key for lookup.

Simply start a new stopwatch, and a stopwatch gets added to the collection and is started.
Stop, reset, or remove any stopwatch using a key generated at the time it was started.

The stopwatch manager supports Microsoft.Extensions.Logging loggers and, by default,
logs when a stopwatch is started or stopped. Elapsed time is logged when the watch is stopped, by default.

There are also options to suppress logging, as stopping a stopwatch also returns the elapsed time via
an out TimeSpan parameter.

This may not be a library you'll ship to production, but it will help you get to production faster
by allowing complex time recordings for operations you perform in your source code.

You can get a copy of this NuGet from the following location: [https://www.nuget.org/packages/stopwatch-manager/](https://www.nuget.org/packages/stopwatch-manager/)

## Installation

Simply add the NuGet package to an existing .NET 6 or 7 project:

```bash
dotnet add package stopwatch-manager
```

or manually add the XML yourself to your .csproj file:

```xml
<PackageReference Include="stopwatch-manager" Version="<current_version>" />
```

The releases on this GitHub repo mirror the versions published on nuget.org, so feel
free to use the current release version from the repo when adding XML manually
to your project definition.

## Usage

To use the stopwatch manager, create a logger instance, create a StopwatchManager instance,
and start a stopwatch:

```csharp
using var loggerFactory =
  LoggerFactory.Create(builder =>
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss";
    }));

var logger = loggerFactory.CreateLogger<ClassName>();

var stopwatchManager = new StopwatchManager(logger);
bool started = stopwatchManager.TryStart(out var eventKey)
```

Note a unique key composed of `<caller>_<line-number>` will be generated by the call
to TryStart() and will be returned as an out parameter. The caller is the method, property,
or event from which the call originated. Use this value to reference the stopwatch
in subsequent calls.

You may also choose not to provide a logger, preventing Stopwatch Manager from logging at all.
This is a good option if you plan to log results yourself:

```csharp
var stopwatchManager = new StopwatchManager();
bool started = stopwatchManager.TryStart(out var eventKey)
```

Stopwatch manager works with any Microsoft.Extensions.Logging logger instance
and logs when a stopwatch is started or stopped. When stopped, the log includes the
elapsed milliseconds of the stopwatch at the time it was stopped.

Use the generated event key you received from your TryStart() call to stop the stopwatch.

```csharp
bool stopped = stopwatchManager.TryStop(eventKey);
```

You may also send in your own unique event key instead of allowing one to be generated for you.
Stopwatch manager can also list all of the stopwatches currently in the collection,
and you can reset or remove stopwatches that are already in the collection.
