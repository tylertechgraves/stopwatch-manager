using Microsoft.Extensions.Logging;

namespace stopwatch_manager.integration_tests;

public partial class Program
{
    public static void Main()
    {
        using var loggerFactory =
        LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }));

        var logger = loggerFactory.CreateLogger<Program>();

        var stopwatchManager = new StopwatchManager(logger);
        var started = stopwatchManager.TryStart(out var key1);
        if (!started)
            Console.WriteLine("Stopwatch 1 was not started");
        Thread.Sleep(1000);
        stopwatchManager.TryStart(out var key2);
        Thread.Sleep(1000);
        stopwatchManager.TryStart(out var key3);
        Thread.Sleep(1000);
        stopwatchManager.TryStart(out var key4);
        Thread.Sleep(1000);
        stopwatchManager.TryStart(out var key5);
        Thread.Sleep(1000);
        stopwatchManager.LogStopwatchList();
        stopwatchManager.Reset(key1);
        stopwatchManager.TryStart(key1);
        stopwatchManager.TryStop(key1);
        stopwatchManager.TryStopAndRemove(key2);
        stopwatchManager.Restart(key3);
        stopwatchManager.TryStop(key3);
        stopwatchManager.TryStop(key4);
        stopwatchManager.TryStop(key5);
        var stopwatchKeys = stopwatchManager.GetStopwatchKeys();
    }
}
