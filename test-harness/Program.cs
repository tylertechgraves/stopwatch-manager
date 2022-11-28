﻿using Microsoft.Extensions.Logging;
using stopwatch_manager;

namespace test_harness;

public partial class Program
{
  public static void Main(string[] args)
  {
    using ILoggerFactory loggerFactory =
    LoggerFactory.Create(builder =>
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        }));

    var logger = loggerFactory.CreateLogger<Program>();
    
    var stopwatchManager = new StopwatchManager(logger);
    stopwatchManager.TryStart("1");
    Thread.Sleep(1000);
    stopwatchManager.TryStart("2");
    Thread.Sleep(1000);
    stopwatchManager.TryStart("3");
    Thread.Sleep(1000);
    stopwatchManager.TryStart("4");
    Thread.Sleep(1000);
    stopwatchManager.TryStart("5");
    Thread.Sleep(1000);
    stopwatchManager.TryStop("1");
    stopwatchManager.TryStop("2");
    stopwatchManager.TryStop("3");
    stopwatchManager.TryStop("4");
    stopwatchManager.TryStop("5");
  }
}