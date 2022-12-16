namespace stopwatch_manager;

/// <summary>Enumeration that determines the type of measurement that appears in logs</summary>
public enum TimespanGranularity
{
  /// <summary>The number of elapsed ticks</summary>
  Ticks = 0,
  /// <summary>The number of elapsed milliseconds</summary>
  Milliseconds,

}