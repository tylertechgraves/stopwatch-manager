using Microsoft.Extensions.Logging;
using Moq;

namespace stopwatch_manager.Tests;

public class StopwatchManagerTests : BaseTest
{
  private static void AssertLogs(Mock<ILogger<StopwatchManager>> logger, string type, string[] expectedLogs)
  {
      Assert.All(expectedLogs, m => Assert.Contains(m, logger.Invocations.Where(i => i.Arguments[0].ToString() == type).Select(i => i.Arguments[2].ToString())));
      Assert.All(logger.Invocations.Where(i => i.Arguments[0].ToString() == type).Select(i => i.Arguments[2].ToString()), m => Assert.Contains(m, expectedLogs));
  }

  private IEnumerable<IInvocation> GetLogsThatStartWith(Mock<ILogger<StopwatchManager>> logger, string type, string logPrefix)
  {
#pragma warning disable CS8602
    return logger.Invocations.Where(i => i.Arguments[0].ToString() == type && i.Arguments[2].ToString().StartsWith(logPrefix));
#pragma warning restore CS8602
  }

  #region TestTryStart
  public class TestTryStartParams : TestParams
  {
    public bool Started { get; set; } = true;
    public string EventKey { get; set; } = string.Empty;
    public bool SendNullLogger { get; set; } = false;
    public bool SendNoLogger { get; set; } = false;
    public int FinalLogCount { get; set; } = 0;
    public string BeginningOfElapsedLog { get; set; } = string.Empty;
  }

  public static TheoryData<TestTryStartParams> TestTryStartParamsData =>
    new()
    {
      new TestTryStartParams
      {
          CaseName = "Successful start",
          EventKey = "TestTryStart_81",
          FinalLogCount = 2,
          BeginningOfElapsedLog = "TIMELOG_ELAPSED: TestTryStart_81"
      },
      new TestTryStartParams
      {
          CaseName = "Successful start with null logger",
          EventKey = "TestTryStart_81",
          SendNullLogger = true,
      },
      new TestTryStartParams
      {
          CaseName = "Successful start with no logger",
          EventKey = "TestTryStart_81",
          SendNoLogger = true,
      },
    };

  [Theory]
  [MemberData(nameof(TestTryStartParamsData))]
  public void TestTryStart(TestTryStartParams testCase)
  {
    var (_, mockLogger) = NewTypedLogger<StopwatchManager>();
    StopwatchManager stopwatchManager;

#pragma warning disable CS8600
    ILogger nullLogger = null;
#pragma warning restore CS8600

    if (!testCase.SendNullLogger)
    {
      if (testCase.SendNoLogger)
        stopwatchManager = new StopwatchManager();
      else
        stopwatchManager = new StopwatchManager(mockLogger.Object);
    }
    else
    {
#pragma warning disable CS8604
      stopwatchManager = new StopwatchManager(nullLogger);
#pragma warning restore CS8604
    }

    var started = stopwatchManager.TryStart(out var eventKey);
    Assert.True(started);
    Assert.NotNull(eventKey);
    Assert.Equal(testCase.EventKey, eventKey);
    if (!testCase.SendNullLogger && !testCase.SendNoLogger)
      AssertLogs(mockLogger, "Information", new string [] { $"TIMELOG: {testCase.EventKey} timer started" });
    else
      AssertLogs(mockLogger, "Information", Array.Empty<string>());
    
    started = stopwatchManager.TryStart(eventKey);
    Assert.False(started);

    var stopped = stopwatchManager.TryStop(eventKey);
    Assert.True(stopped);
    Assert.Equal(mockLogger.Invocations.Count, testCase.FinalLogCount);
    if (!string.IsNullOrEmpty(testCase.BeginningOfElapsedLog))
      Assert.Single(GetLogsThatStartWith(mockLogger, "Information", testCase.BeginningOfElapsedLog));

    var removed = stopwatchManager.TryStopAndRemove(eventKey, out var timespan);
    Assert.True(removed);
    Assert.IsType<TimeSpan>(timespan);

    removed = stopwatchManager.TryStopAndRemove(eventKey);
    Assert.False(removed);

    stopped = stopwatchManager.TryStopNoLog("fakekey", out timespan);
    Assert.False(stopped);
    Assert.IsType<TimeSpan>(timespan);

    var result = stopwatchManager.TryStopAndRemove("fakekey");
    Assert.False(result);
  }
  #endregion

  #region TestTryStartNoLog
  public class TestTryStartNoLogParams : TestParams
  {
    public bool Started { get; set; } = true;
    public string EventKey { get; set; } = string.Empty;
    public bool SendNullLogger { get; set; } = false;
    public bool SendNoLogger { get; set; } = false;
  }

  public static TheoryData<TestTryStartNoLogParams> TestTryStartNoLogParamsData =>
    new()
    {
      new TestTryStartNoLogParams
      {
          CaseName = "Successful stopwatch start",
          EventKey = "TestTryStartNoLog_171",
      },
      new TestTryStartNoLogParams
      {
          CaseName = "Successful stopwatch start",
          EventKey = "TestTryStartNoLog_171",
          SendNullLogger = true,
      },
      new TestTryStartNoLogParams
      {
          CaseName = "Successful stopwatch start",
          EventKey = "TestTryStartNoLog_171",
          SendNoLogger = true,
      },
    };

  [Theory]
  [MemberData(nameof(TestTryStartNoLogParamsData))]
  public void TestTryStartNoLog(TestTryStartNoLogParams testCase)
  {
    var (_, mockLogger) = NewTypedLogger<StopwatchManager>();
    StopwatchManager stopwatchManager;

#pragma warning disable CS8600
    ILogger nullLogger = null;
#pragma warning restore CS8600

    if (!testCase.SendNullLogger)
    {
      if (testCase.SendNoLogger)
        stopwatchManager = new StopwatchManager();
      else
        stopwatchManager = new StopwatchManager(mockLogger.Object);
    }
    else
    {
#pragma warning disable CS8604
      stopwatchManager = new StopwatchManager(nullLogger);
#pragma warning restore CS8604
    }

    var started = stopwatchManager.TryStartNoLog(out var eventKey);
    Assert.True(started);
    Assert.NotNull(eventKey);
    Assert.Equal(testCase.EventKey, eventKey);
    
    started = stopwatchManager.TryStartNoLog(eventKey);
    Assert.False(started);

    var stopped = stopwatchManager.TryStopNoLog(eventKey, out var timespan);
    Assert.True(stopped);
    Assert.IsType<TimeSpan>(timespan);

    var removed = stopwatchManager.TryStopAndRemoveNoLog(eventKey);
    Assert.True(removed);

    var result = stopwatchManager.TryStop(eventKey, out timespan);
    Assert.False(result);
    Assert.IsType<TimeSpan>(timespan);

    AssertLogs(mockLogger, "Information", Array.Empty<string>());
  }
  #endregion

  [Fact]
  public void TestReset()
  {
    var (_, mockLogger) = NewTypedLogger<StopwatchManager>();
    var stopwatchManager = new StopwatchManager(mockLogger.Object, "TEST_LOG_PREFIX", "TEST_LOGPREFIX_ELAPSED");
    Assert.True(stopwatchManager.TryStart("testevent"));
    AssertLogs(mockLogger, "Information", new string [] { "TEST_LOG_PREFIX: testevent timer started" });
    Assert.True(stopwatchManager.Reset("testevent"));
    Assert.False(stopwatchManager.Reset("incorrectTestEvent"));
    Assert.True(stopwatchManager.TryStop("testevent"));
    Assert.Single(GetLogsThatStartWith(mockLogger, "Information", "TEST_LOGPREFIX_ELAPSED: testevent"));
  }

  [Fact]
  public void TestRestart()
  {
    var (_, mockLogger) = NewTypedLogger<StopwatchManager>();
    var stopwatchManager = new StopwatchManager("TEST_LOG_PREFIX", "TEST_LOGPREFIX_ELAPSED");
    Assert.True(stopwatchManager.TryStart("testevent"));
    Assert.True(stopwatchManager.Restart("testevent"));
    Assert.False(stopwatchManager.Restart("incorrectTestEvent"));
    Assert.True(stopwatchManager.TryStop("testevent"));
    AssertLogs(mockLogger, "Information", Array.Empty<string>());
  }
}
