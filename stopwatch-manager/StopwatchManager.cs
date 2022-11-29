using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace stopwatch_manager;

/// <summary>
/// The Stopwatch Manager is a thread-safe NuGet that manages a collection
/// of stopwatches. Each stopwatch has a unique string key associated with it,
/// and each can be managed independently.
/// </summary>
public class StopwatchManager
{
    private readonly ConcurrentDictionary<string, Stopwatch> _stopwatches;
    private readonly Serilog.ILogger? _serilogLogger;
    private readonly ILogger? _msLogger;
    private const string LOG_PREFIX = "TIMELOG";
    private const string LOG_PREFIX_ELAPSED = "TIMELOG_ELAPSED";
    private string _logPrefix = LOG_PREFIX;
    private string _logPrefixElapsed = LOG_PREFIX_ELAPSED;

    /// <summary>
    /// Stopwatch manager constructor
    /// </summary>
    /// <param name="logger">A Serilog ILogger instance that can be used for logging in the consuming application</param>
    public StopwatchManager(Serilog.ILogger logger)
    {
        _serilogLogger = logger;
        _msLogger = null;
        _stopwatches = new();
    }

    /// <summary>
    /// Stopwatch manager constructor
    /// </summary>
    /// <param name="logger">A Serilog ILogger instance that can be used for logging in the consuming application</param>
    /// <param name="logPrefix">The string used to prefix initial stopwatch logs</param>
    /// <param name="logPrefixElapsed">A string used to prefix elapsed time stopwatch logs</param>
    public StopwatchManager(Serilog.ILogger logger, string logPrefix, string logPrefixElapsed)
    {
        _serilogLogger = logger;
        _msLogger = null;
        _stopwatches = new();
        _logPrefix = logPrefix;
        _logPrefixElapsed = logPrefixElapsed;
    }

    /// <summary>
    /// Stopwatch manager constructor
    /// </summary>
    /// <param name="logger">A Microsoft.Extensions.Logging.ILogger instance that can be used for logging in the consuming application</param>
    public StopwatchManager(ILogger logger)
    {
        _serilogLogger = null;
        _msLogger = logger;
        _stopwatches = new();
    }

    /// <summary>
    /// Stopwatch manager constructor
    /// </summary>
    /// <param name="logger">A Microsoft.Extensions.Logging.ILogger instance that can be used for logging in the consuming application</param>
    /// <param name="logPrefix">The string used to prefix initial stopwatch logs</param>
    /// <param name="logPrefixElapsed">A string used to prefix elapsed time stopwatch logs</param>
    public StopwatchManager(ILogger logger, string logPrefix, string logPrefixElapsed)
    {
        _serilogLogger = null;
        _msLogger = logger;
        _stopwatches = new();
        _logPrefix = logPrefix;
        _logPrefixElapsed = logPrefixElapsed;
    }

    /// <summary>
    /// This method will add a new stopwatch to the collection, using
    /// <paramref name="eventDescription"/> as its key, and will start the timer.
    /// If a stopwatch with a key of <paramref name="eventDescription"/> already exists,
    /// this method takes no action. Once the stopwatch is started,
    /// a log will be written indicating as such.
    /// </summary>
    /// <param name="eventDescription">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch is not found, is added, and is started.
    /// True if stopped stopwatch is found and is restarted. False otherwise.</returns>
    public bool TryStart(string eventDescription)
    {
        return TryStart(eventDescription, true);
    }

    /// <summary>
    /// This method will add a new stopwatch to the collection, using
    /// <paramref name="eventDescription"/> as its key, and will start the timer.
    /// If a stopwatch with a key of <paramref name="eventDescription"/> already exists,
    /// this method takes no action.
    /// </summary>
    /// <param name="eventDescription">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch is not found, is added, and is started.
    /// True if stopped stopwatch is found and is restarted. False otherwise.</returns>
    public bool TryStartNoLog(string eventDescription)
    {
        return TryStart(eventDescription, false);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventDescription"/>
    /// and will stop it if found. The stopwatch will not be removed from the collection
    /// and will not be reset. If the stopwatch is not found, this method takes no action.
    /// Once stopped, this method logs the elapsed time of the stopped stopwatch.
    /// </summary>
    /// <param name="eventDescription">Name of event being timed; used as key in stopwatch collection</param>
    /// <param name="timespan">Provides timespan measurement of stopped stopwatch</param>
    /// <returns>True if stopwatch was found and stopped; otherwise, false.</returns>
    public bool TryStop(string eventDescription, out TimeSpan timespan)
    {
        return TryStop(eventDescription, out timespan, true, false);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventDescription"/>
    /// and will stop it if found. The stopwatch will not be removed from the collection
    /// and will not be reset. If the stopwatch is not found, this method takes no action.
    /// Once stopped, this method logs the elapsed time of the stopped stopwatch.
    /// </summary>
    /// <param name="eventDescription">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch was found and stopped; otherwise, false.</returns>
    public bool TryStop(string eventDescription)
    {
        return TryStop(eventDescription, out _, true, false);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventDescription"/>
    /// and will stop it if found. The stopwatch will not be removed from the collection
    /// and will not be reset. If the stopwatch is not found, this method takes no action.
    /// </summary>
    /// <param name="eventDescription">Name of event being timed; used as key in stopwatch collection</param>
    /// <param name="timespan">Provides timespan measurement of stopped stopwatch</param>
    /// <returns>True if stopwatch was found and stopped; otherwise, false.</returns>
    public bool TryStopNoLog(string eventDescription, out TimeSpan timespan)
    {
        var stopped = TryStopStopwatch(eventDescription, out timespan, false);
        if (!stopped)
            return false;

        return true;
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventDescription"/>
    /// and will stop it if found. The stopwatch will be removed from the collection.
    /// If the stopwatch is not found, this method takes no action.
    /// Once stopped, this method logs the elapsed time of the stopped stopwatch.
    /// </summary>
    /// <param name="eventDescription">Name of event being timed; used as key in stopwatch collection</param>
    /// <param name="timespan">Provides timespan measurement of stopped stopwatch</param>
    /// <returns>True if stopwatch was found and stopped; otherwise, false.</returns>
    public bool TryStopAndRemove(string eventDescription, out TimeSpan timespan)
    {
        return TryStop(eventDescription, out timespan, true, true);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventDescription"/>
    /// and will stop it if found. The stopwatch will be removed from the collection.
    /// If the stopwatch is not found, this method takes no action.
    /// Once stopped, this method logs the elapsed time of the stopped stopwatch.
    /// </summary>
    /// <param name="eventDescription">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch was found and stopped; otherwise, false.</returns>
    public bool TryStopAndRemove(string eventDescription)
    {
        return TryStop(eventDescription, out _, true, true);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventDescription"/>
    /// and will stop it if found. The stopwatch will be removed from the collection.
    /// If the stopwatch is not found, this method takes no action.
    /// </summary>
    /// <param name="eventDescription">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch was found and stopped; otherwise, false.</returns>
    public bool TryStopAndRemoveNoLog(string eventDescription)
    {
        return TryStop(eventDescription, out _, false, true);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventDescription"/>
    /// and will reset it if found. If the stopwatch is not found, this method takes no action.
    /// </summary>
    /// <param name="eventDescription">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch was found and reset; otherwise, false.</returns>
    public bool Reset(string eventDescription)
    {
        var found = _stopwatches.TryGetValue(eventDescription, out var stopwatch);
        if (found)
        {
            stopwatch?.Reset();
            return true;
        }

        return false;
    }

    private bool TryStart(string eventDescription, bool writeLog)
    {
        var started = TryStartStopwatch(eventDescription);
        if (!started)
            return false;

        if (writeLog)
            LogStart("{prefix}: {eventDescription} timer started", LOG_PREFIX, eventDescription);

        return true;
    }

    private bool TryStop(string eventDescription, out TimeSpan timespan, bool writeLogResult, bool remove)
    {
        var stopped = TryStopStopwatch(eventDescription, out timespan, remove);
        if (!stopped)
            return false;

        if (writeLogResult)
            LogResult("{prefix}: {eventDescription} {duration}", LOG_PREFIX_ELAPSED, eventDescription, timespan);

        return true;
    }

    private bool TryStartStopwatch(string key)
    {
        var stopwatch = _stopwatches.GetOrAdd(key, new Stopwatch());
        if (!stopwatch.IsRunning)
        {
            stopwatch.Start();
            return true;
        }

        return false;
    }

    private bool TryStopStopwatch(string key, out TimeSpan timespan, bool remove)
    {
        var found = _stopwatches.TryGetValue(key, out var stopwatch);
        if (found)
        {
            if (stopwatch != null)
            {
                stopwatch.Stop();
                timespan = stopwatch.Elapsed;
                stopwatch.Reset();

                if (remove)
                    _stopwatches.Remove(key, out _);

                return true;
            }
        }

        timespan = new TimeSpan();
        return false;
    }

#pragma warning disable CA2254
    private void LogStart(string description, string logPrefix, string eventDescription)
    {
        if (_serilogLogger != null)
        {
            _serilogLogger.Information(description, logPrefix, eventDescription);
            return;
        }

        _msLogger?.LogInformation(description, logPrefix, eventDescription);
    }

    private void LogResult(string description, string logPrefix, string eventDescription, TimeSpan timespan)
    {
        if (_serilogLogger != null)
        {
            _serilogLogger.Information(description, logPrefix, eventDescription, timespan.TotalMilliseconds);
            return;
        }

        _msLogger?.LogInformation(description, logPrefix, eventDescription, timespan.TotalMilliseconds);
    }
#pragma warning restore CA2254
}
