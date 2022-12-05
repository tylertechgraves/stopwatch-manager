# Stopwatch Manager for .NET

Have you ever had to record timings for a multi-threaded application and realized you needed
to record the length of so many events that you actually need a collection of
stopwatches instead of a single stopwatch? Yeah, me too.

That's why I developed the Stopwatch Manager for .NET.  The package supports .NET 6 & 7, and
it creates a concurrent dictionary of stopwatches, each with its own unique key for lookup.

Simply start a new stopwatch with a unique key name you haven't previously used, and a stopwatch
gets added to the collection and is started. Stop, reset, or remove any stopwatch using the same
key you used to start it.

The stopwatch manager supports Microsoft.Extensions.Logging and Serilog loggers and, by default,
logs when a stopwatch is started or stopped. Elapsed time is logged when the watch is stopped, by default.

There are also options to suppress logging, as stopping a stopwatch also returns the elapsed time via
an out TimeSpan parameter.

This may not be a library you'll ship to production, but it will help you get to production faster
by allowing complex time recordings for operations you perform in your source code.

You can get a copy of this NuGet from the following location: [https://www.nuget.org/packages/stopwatch-manager/](https://www.nuget.org/packages/stopwatch-manager/)
