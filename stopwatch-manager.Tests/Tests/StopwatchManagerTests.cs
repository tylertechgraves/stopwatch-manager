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

    Assert.True(stopwatchManager.TryStart(out var eventKey));
    Assert.NotNull(eventKey);
    Assert.Equal(testCase.EventKey, eventKey);
    if (!testCase.SendNullLogger && !testCase.SendNoLogger)
      AssertLogs(mockLogger, "Information", new string[] { $"TIMELOG: {testCase.EventKey} timer started" });
    else
      AssertLogs(mockLogger, "Information", Array.Empty<string>());

    Assert.False(stopwatchManager.TryStart(eventKey));

    Assert.True(stopwatchManager.TryStop(eventKey, TimespanGranularity.Ticks));
    Assert.Equal(mockLogger.Invocations.Count, testCase.FinalLogCount);
    if (!string.IsNullOrEmpty(testCase.BeginningOfElapsedLog))
      Assert.Single(GetLogsThatStartWith(mockLogger, "Information", testCase.BeginningOfElapsedLog));

    Assert.True(stopwatchManager.TryStopAndRemove(eventKey, out var timespan));
    Assert.IsType<TimeSpan>(timespan);

    Assert.False(stopwatchManager.TryStopAndRemove(eventKey));

    Assert.False(stopwatchManager.TryStopNoLog("fakekey", out timespan));
    Assert.IsType<TimeSpan>(timespan);

    Assert.False(stopwatchManager.TryStopAndRemove("fakekey"));
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
          EventKey = "TestTryStartNoLog_164",
      },
      new TestTryStartNoLogParams
      {
          CaseName = "Successful stopwatch start",
          EventKey = "TestTryStartNoLog_164",
          SendNullLogger = true,
      },
      new TestTryStartNoLogParams
      {
          CaseName = "Successful stopwatch start",
          EventKey = "TestTryStartNoLog_164",
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

    Assert.False(stopwatchManager.TryStartNoLog(eventKey));

    Assert.True(stopwatchManager.TryStopNoLog(eventKey, out var timespan));
    Assert.IsType<TimeSpan>(timespan);

    Assert.True(stopwatchManager.TryStopAndRemoveNoLog(eventKey));

    Assert.False(stopwatchManager.TryStop(eventKey, out timespan));
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
    AssertLogs(mockLogger, "Information", new string[] { "TEST_LOG_PREFIX: testevent timer started" });
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

  [Fact]
  public void TestLogStopwatchList()
  {
    var (_, mockLogger) = NewTypedLogger<StopwatchManager>();
    var stopwatchManager = new StopwatchManager(mockLogger.Object);
    Assert.True(stopwatchManager.TryStart("testevent"));
    Thread.Sleep(100);
    Assert.True(stopwatchManager.TryStart("testevent2"));
    AssertLogs(mockLogger, "Information", new string[] { "TIMELOG: testevent timer started", "TIMELOG: testevent2 timer started" });
    stopwatchManager.LogStopwatchList();

    // Make sure we got 3 logs
    Assert.Equal(3, mockLogger.Invocations.Where(i => i.Arguments[0].ToString() == "Information").Count());

    // Create a string array, split on \n
    var logString = mockLogger.Invocations[2].Arguments[2].ToString();
    var stringArray = logString?.Split('\n');
    Assert.NotNull(stringArray);

    Assert.Equal("testevent", stringArray[0]);
    Assert.Equal("testevent2", stringArray[3]);
    Assert.Empty(stringArray[2]);
    if (double.TryParse(stringArray[4], out var firstElapsedTime))
      Assert.IsType<double>(firstElapsedTime);
    else
      Assert.Fail("First elapsed time was not a double");
    if (double.TryParse(stringArray[1], out var secondElapsedTime))
      Assert.IsType<double>(secondElapsedTime);
    else
      Assert.Fail("Second elapsed time was not a double");
  }

  [Fact]
  public void TestLogStopwatchListNoLogger()
  {
    var stopwatchManager = new StopwatchManager();
    Assert.True(stopwatchManager.TryStart("testevent"));
    Thread.Sleep(100);
    Assert.True(stopwatchManager.TryStart("testevent2"));
    stopwatchManager.LogStopwatchList();
  }

  [Fact]
  public void TestGetStopwatchKeys()
  {
    var stopwatchManager = new StopwatchManager();
    Assert.True(stopwatchManager.TryStart("testevent"));
    Assert.True(stopwatchManager.TryStart("testevent2"));
    var keyList = stopwatchManager.GetStopwatchKeys();
    Assert.Equal(new List<string> { "testevent", "testevent2" }, keyList);
  }
}
