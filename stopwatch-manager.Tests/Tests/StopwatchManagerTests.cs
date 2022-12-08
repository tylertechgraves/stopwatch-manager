using Microsoft.Extensions.Logging;
using Moq;

namespace stopwatch_manager.Tests;

public class StopwatchManagerTests : BaseTest
{

  #region TestTryStart
  public class TestTryStartParams : TestParams
  {
    public bool Started { get; set; } = true;
    public string EventKey { get; set; } = string.Empty;
  }

  public static TheoryData<TestTryStartParams> TestTryStartParamsData =>
    new()
    {
      new TestTryStartParams
      {
          CaseName = "Successful stopwatch start",
          EventKey = "TestTryStart_34",
      },
    };

  [Theory]
  [MemberData(nameof(TestTryStartParamsData))]
  public void TestTryStart(TestTryStartParams testCase)
  {
    var (_, mockLogger) = NewTypedLogger<StopwatchManager>();

    var stopwatchManager = new StopwatchManager(mockLogger.Object);

    stopwatchManager.TryStart(out var eventKey);
    Assert.NotNull(eventKey);
    Assert.Equal(testCase.EventKey, eventKey);
  }
  #endregion

  #region TestTryStartNoLog
  [Fact]
  public void TestTryStartNoLog()
  {

  }
  #endregion
}
