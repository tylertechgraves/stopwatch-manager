using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    private readonly ILogger? _msLogger;
    private const string LOG_PREFIX = "TIMELOG";
    private const string LOG_PREFIX_ELAPSED = "TIMELOG_ELAPSED";
    private readonly string _logPrefix = LOG_PREFIX;
    private readonly string _logPrefixElapsed = LOG_PREFIX_ELAPSED;

    /// <summary>
    /// Stopwatch manager constructor
    /// </summary>
    /// <param name="logger">A Microsoft.Extensions.Logging.ILogger instance that can be used for logging in the consuming application</param>
    public StopwatchManager(ILogger logger)
    {
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
        _msLogger = logger;
        _stopwatches = new();
        _logPrefix = logPrefix;
        _logPrefixElapsed = logPrefixElapsed;
    }

    /// <summary>
    /// Stopwatch manager constructor. If you use this constructor, logs will never be written.
    /// </summary>
    public StopwatchManager()
    {
        _msLogger = null;
        _stopwatches = new();
    }

    /// <summary>
    /// Stopwatch manager constructor. If you use this constructor, logs will never be written.
    /// </summary>
    /// <param name="logPrefix">The string used to prefix initial stopwatch logs</param>
    /// <param name="logPrefixElapsed">A string used to prefix elapsed time stopwatch logs</param>
    public StopwatchManager(string logPrefix, string logPrefixElapsed)
    {
        _msLogger = null;
        _stopwatches = new();
        _logPrefix = logPrefix;
        _logPrefixElapsed = logPrefixElapsed;
    }

    /// <summary>
    /// This method will add a new stopwatch to the collection, using
    /// <paramref name="eventKey"/> as its key, and will start it.
    /// If a stopwatch with a key of <paramref name="eventKey"/> already exists and is already running,
    /// this method takes no action. If the stopwatch is found but is not running, the stopwatch
    /// will be started.  Once the stopwatch is started, a log will be written indicating as such.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch is not found, is added, and is started.
    /// True if stopped stopwatch is found, is not running, and is started. False otherwise.</returns>
    public bool TryStart(string eventKey)
    {
        return TryStart(eventKey, true);
    }

    /// <summary>
    /// This method will add a new stopwatch to the collection, using
    /// CompilerServices to determine the caller class and line number.
    /// The generated event key will be returned in the <paramref name="eventKey"/> out
    /// parameter. If a stopwatch with a key of <paramref name="eventKey"/> already exists and is already running,
    /// this method takes no action. If the stopwatch is found but is not running, the stopwatch
    /// will be started.  Once the stopwatch is started, a log will be written indicating as such.
    /// </summary>
    /// <param name="eventKey">Out parameter that will return an event key generated from caller identity information</param>
    /// <param name="memberName">Optional parameter that defaults to the caller's class name</param>
    /// <param name="sourceLineNumber">Optional parameter that defaults to the line number where this function is being called</param>
    /// <returns>True if stopwatch is not found, is added, and is started.
    /// True if stopped stopwatch is found, is not running, and is started. False otherwise.</returns>
    public bool TryStart(out string eventKey,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        eventKey = memberName + "_" + sourceLineNumber;
        return TryStart(eventKey, true);
    }

    /// <summary>
    /// This method will add a new stopwatch to the collection, using
    /// <paramref name="eventKey"/> as its key, and will start it.
    /// If a stopwatch with a key of <paramref name="eventKey"/> already exists and is already running,
    /// this method takes no action. If the stopwatch is found but is not running, the stopwatch
    /// will be started.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch is not found, is added, and is started.
    /// True if stopped stopwatch is found, is not running, and is started. False otherwise.</returns>
    public bool TryStartNoLog(string eventKey)
    {
        return TryStart(eventKey, false);
    }

    /// <summary>
    /// This method will add a new stopwatch to the collection, using
    /// <paramref name="eventKey"/> as its key, and will start it.
    /// If a stopwatch with a key of <paramref name="eventKey"/> already exists and is already running,
    /// this method takes no action. If the stopwatch is found but is not running, the stopwatch
    /// will be started.
    /// </summary>
    /// <param name="eventKey">Out parameter that will return an event key generated from caller identity information</param>
    /// <param name="memberName">Optional parameter that defaults to the caller's class name</param>
    /// <param name="sourceLineNumber">Optional parameter that defaults to the line number where this function is being called</param>
    /// <returns>True if stopwatch is not found, is added, and is started.
    /// True if stopped stopwatch is found, is not running, and is started. False otherwise.</returns>
    public bool TryStartNoLog(out string eventKey,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        eventKey = memberName + "_" + sourceLineNumber;
        return TryStart(eventKey, false);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventKey"/>
    /// and will stop it if found. The stopwatch will not be removed from the collection
    /// and will not be reset. If the stopwatch is not found, this method takes no action.
    /// Once stopped, this method logs the elapsed time of the stopped stopwatch.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <param name="timespan">Provides timespan measurement of stopped stopwatch</param>
    /// <param name="granularity">Enumeration that determines the type of measurement that appears in logs</param>
    /// <returns>True if stopwatch was found and stopped. False otherwise.</returns>
    public bool TryStop(string eventKey, out TimeSpan timespan, TimespanGranularity granularity = TimespanGranularity.Milliseconds)
    {
        return TryStop(eventKey, out timespan, true, false, granularity);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventKey"/>
    /// and will stop it if found. The stopwatch will not be removed from the collection
    /// and will not be reset. If the stopwatch is not found, this method takes no action.
    /// Once stopped, this method logs the elapsed time of the stopped stopwatch.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <param name="granularity">Enumeration that determines the type of measurement that appears in logs</param>
    /// <returns>True if stopwatch was found and stopped. False otherwise.</returns>
    public bool TryStop(string eventKey, TimespanGranularity granularity = TimespanGranularity.Milliseconds)
    {
        return TryStop(eventKey, out _, true, false, granularity);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventKey"/>
    /// and will stop it if found. The stopwatch will not be removed from the collection
    /// and will not be reset. If the stopwatch is not found, this method takes no action.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <param name="timespan">Provides timespan measurement of stopped stopwatch</param>
    /// <returns>True if stopwatch was found and stopped. False otherwise.</returns>
    public bool TryStopNoLog(string eventKey, out TimeSpan timespan)
    {
        var stopped = TryStopStopwatch(eventKey, out timespan, false);
        if (!stopped)
            return false;

        return true;
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventKey"/>
    /// and will stop it if found. The stopwatch will be removed from the collection.
    /// If the stopwatch is not found, this method takes no action.
    /// Once stopped, this method logs the elapsed time of the stopped stopwatch.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <param name="timespan">Provides timespan measurement of stopped stopwatch</param>
    /// <param name="granularity">Enumeration that determines the type of measurement that appears in logs</param>
    /// <returns>True if stopwatch was found and stopped. False otherwise.</returns>
    public bool TryStopAndRemove(string eventKey, out TimeSpan timespan, TimespanGranularity granularity = TimespanGranularity.Milliseconds)
    {
        return TryStop(eventKey, out timespan, true, true, granularity);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventKey"/>
    /// and will stop it if found. The stopwatch will be removed from the collection.
    /// If the stopwatch is not found, this method takes no action.
    /// Once stopped, this method logs the elapsed time of the stopped stopwatch.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <param name="granularity">Enumeration that determines the type of measurement that appears in logs</param>
    /// <returns>True if stopwatch was found and stopped. False otherwise.</returns>
    public bool TryStopAndRemove(string eventKey, TimespanGranularity granularity = TimespanGranularity.Milliseconds)
    {
        return TryStop(eventKey, out _, true, true, granularity);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventKey"/>
    /// and will stop it if found. The stopwatch will be removed from the collection.
    /// If the stopwatch is not found, this method takes no action.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch was found and stopped. False otherwise.</returns>
    public bool TryStopAndRemoveNoLog(string eventKey)
    {
        return TryStop(eventKey, out _, false, true, TimespanGranularity.Milliseconds);
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventKey"/>
    /// and will reset it if found. If the stopwatch is not found, this method takes no action.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch was found and reset. False otherwise.</returns>
    public bool Reset(string eventKey)
    {
        var found = _stopwatches.TryGetValue(eventKey, out var stopwatch);
        if (found)
        {
            stopwatch?.Reset();
            return true;
        }

        return false;
    }

    /// <summary>
    /// This method will find the stopwatch with key <paramref name="eventKey"/>
    /// and will reset and start it if found. If the stopwatch is not found, this method takes no action.
    /// </summary>
    /// <param name="eventKey">Name of event being timed; used as key in stopwatch collection</param>
    /// <returns>True if stopwatch was found, reset, and started. False otherwise.</returns>
    public bool Restart(string eventKey)
    {
        var found = _stopwatches.TryGetValue(eventKey, out var stopwatch);
        if (found)
        {
            stopwatch?.Reset();
            stopwatch?.Start();
            return true;
        }

        return false;
    }

    /// <summary>
    /// This method logs each stopwatch key and its current elapsed milliseconds;
    /// stopwatches are ordered by their elapsed milliseconds descending.
    /// </summary>
    public void LogStopwatchList()
    {
        if (_msLogger == null)
            return;

        var stopwatchListing = string.Empty;
        foreach (var stopwatch in _stopwatches.OrderByDescending(sw => sw.Value.ElapsedMilliseconds))
        {
            stopwatchListing += stopwatch.Key + "\n";
            stopwatchListing += stopwatch.Value.ElapsedMilliseconds + "\n\n";
        }
        // Remove trailing \n\n
        stopwatchListing = stopwatchListing[..^2];

        _msLogger?.LogInformation("{stopwatchListing}", stopwatchListing);
    }

    /// <summary>
    /// This method returns an ordered list of the keys in the stopwatch collection
    /// </summary>
    /// <returns>List&lt;string&gt; containing keys for all stopwatches, ordered by key value</returns>
    public List<string> GetStopwatchKeys()
    {
        var returnList = new List<string>();
        returnList.AddRange(_stopwatches.Keys);
        returnList.Sort();
        return returnList;
    }

    private bool TryStart(string eventKey, bool writeLog)
    {
        var started = TryStartStopwatch(eventKey);
        if (!started)
            return false;

        if (writeLog)
            LogStart("{prefix}: {eventKey} timer started", eventKey);

        return true;
    }

    private bool TryStop(string eventKey, out TimeSpan timespan, bool writeLogResult, bool remove, TimespanGranularity granularity)
    {
        var stopped = TryStopStopwatch(eventKey, out timespan, remove);
        if (!stopped)
            return false;

        if (writeLogResult)
            LogResult("{prefix}: {eventKey} {duration}", eventKey, timespan, granularity);

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
        _stopwatches.TryGetValue(key, out var stopwatch);
        if (stopwatch != null)
        {
            stopwatch.Stop();
            timespan = stopwatch.Elapsed;
            stopwatch.Reset();

            if (remove)
                _stopwatches.TryRemove(key, out _);

            return true;
        }

        timespan = new TimeSpan();
        return false;
    }

#pragma warning disable CA2254
    private void LogStart(string messageTemplate, string eventKey)
    {
        if (_msLogger == null)
            return;

        _msLogger.LogInformation(messageTemplate, _logPrefix, eventKey);
    }

    private void LogResult(string messageTemplate, string eventKey, TimeSpan timespan, TimespanGranularity granularity)
    {
        if (_msLogger == null)
            return;

        switch (granularity)
        {
            case TimespanGranularity.Milliseconds:
                _msLogger.LogInformation(messageTemplate, _logPrefixElapsed, eventKey, timespan.TotalMilliseconds);
                break;
            case TimespanGranularity.Ticks:
                _msLogger.LogInformation(messageTemplate, _logPrefixElapsed, eventKey, timespan.Ticks);
                break;
            default:
                _msLogger.LogInformation(messageTemplate, _logPrefixElapsed, eventKey, timespan.TotalMilliseconds);
                break;
        }

    }
#pragma warning restore CA2254
}
