using Microsoft.Extensions.Logging;
using Xunit;

namespace stopwatch_manager.integration_tests;

public partial class Program
{
    public static void Main()
    {
        TestWithLogsNoCustomPrefix();
        // TestWithLogsAndCustomPrefix();
        // TestNoLogsNoCustomPrefix();
        // TestNoLogsAndCustomPrefix();
    }

    private static void TestWithLogsNoCustomPrefix()
    {
        // Test case: ILogger used; default log prefixes
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

        Assert.True(stopwatchManager.TryStart(out var key1));
        Assert.NotNull(key1);
        Assert.Equal(key1, $"TestWithLogsNoCustomPrefix_{(new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber() - 2}");
        Thread.Sleep(100);

        Assert.True(stopwatchManager.TryStart(out var key2));
        Assert.NotNull(key2);
        Assert.Equal(key2, $"TestWithLogsNoCustomPrefix_{(new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber() - 2}");
        Thread.Sleep(100);

        Assert.True(stopwatchManager.TryStart(out var key3));
        Assert.NotNull(key3);
        Assert.Equal(key3, $"TestWithLogsNoCustomPrefix_{(new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber() - 2}");
        Thread.Sleep(100);

        Assert.True(stopwatchManager.TryStart(out var key4));
        Assert.NotNull(key4);
        Assert.Equal(key4, $"TestWithLogsNoCustomPrefix_{(new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber() - 2}");
        Thread.Sleep(100);

        stopwatchManager.TryStart(out var key5);
        Assert.NotNull(key5);
        Assert.Equal(key5, $"TestWithLogsNoCustomPrefix_{(new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber() - 2}");
        Thread.Sleep(100);

        stopwatchManager.LogStopwatchList();

        Assert.True(stopwatchManager.Reset(key1));
        Assert.True(stopwatchManager.TryStart(key1));
        Assert.True(stopwatchManager.TryStop(key1));
        Assert.True(stopwatchManager.TryStopAndRemove(key2));
        Assert.True(stopwatchManager.Restart(key3));
        Assert.True(stopwatchManager.TryStop(key3));
        Assert.True(stopwatchManager.TryStop(key4));
        Assert.True(stopwatchManager.TryStop(key5));
        Assert.Equivalent(new List<string> { key1, key3, key4, key5 }, stopwatchManager.GetStopwatchKeys());
    }
}
