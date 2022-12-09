using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace stopwatch_manager.Tests;

public class BaseTest : IDisposable
{
  private readonly ILoggerFactory _logger = new NullLoggerFactory();
  protected ILoggerFactory Logger => _logger;

  protected (ILoggerFactory, Mock<ILogger<T>>) NewTypedLogger<T>()
  {
    var mockLogger = new Mock<ILogger<T>>();
    var mockLoggerFactory = new Mock<ILoggerFactory>();
    mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
    return (mockLoggerFactory.Object, mockLogger);
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
  }
}
