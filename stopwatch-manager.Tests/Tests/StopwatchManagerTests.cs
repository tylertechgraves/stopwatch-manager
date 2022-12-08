using Microsoft.Extensions.Logging;
using Moq;

namespace stopwatch_manager.Tests;

public class StopwatchManagerTests
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
  }

  public static TheoryData<TestTryStartParams> TestTryStartParamsData =>
    new()
    {
      new TestTryStartParams
      {
          CaseName = "Successful stopwatch start",
      },
    };

  [Theory]
  [MemberData(nameof(TestTryStartParamsData))]
  public void TestTryStart(TestTryStartParams testCase)
  {
    
  }
  #endregion

  #region TestTryStartNoLog
  [Fact]
  public void TestTryStartNoLog()
  {

  }
  #endregion
}
