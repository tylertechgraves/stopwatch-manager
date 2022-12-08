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

  #region TestTryStart
  public class TestTryStartParams : TestParams
  {
    public bool Started { get; set; } = true;
    public string EventKey { get; set; } = string.Empty;
    public bool SendNullLogger { get; set; } = false;
  }

  public static TheoryData<TestTryStartParams> TestTryStartParamsData =>
    new()
    {
      new TestTryStartParams
      {
          CaseName = "Successful stopwatch start",
          EventKey = "TestTryStart_58",
      },
      new TestTryStartParams
      {
          CaseName = "Successful stopwatch start",
          EventKey = "TestTryStart_58",
          SendNullLogger = true,
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
      stopwatchManager = new StopwatchManager(mockLogger.Object);
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
    if (!testCase.SendNullLogger)
      AssertLogs(mockLogger, "Information", new string [] { $"TIMELOG: {testCase.EventKey} timer started" });
    else
      AssertLogs(mockLogger, "Information", Array.Empty<string>());
    
    started = stopwatchManager.TryStart(eventKey);
    Assert.False(started);
  }
  #endregion

  // #region TestTryStartNoLog
  // [Fact]
  // public void TestTryStartNoLog()
  // {

  // }
  // #endregion
}
