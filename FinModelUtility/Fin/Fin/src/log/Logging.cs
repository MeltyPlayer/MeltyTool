using System;
using System.Collections.Generic;

using fin.util.asserts;

using Microsoft.Extensions.Logging;

namespace fin.log;

using MicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using FinLogger = ILogger;

public interface ILogger {
  IDisposable BeginScope(string scope);
  void LogInformation(string message);
  void LogWarning(string message);
  void LogError(string message);
}

public class Logging {
  private static bool VERBOSE_ = true;

  private static ILoggerFactory FACTORY_ = LoggerFactory.Create(
      builder =>
          builder //.AddConsole()
              .AddProvider(new CleanConsoleLoggerProvider())
              .AddDebug()
              .AddFilter(
                  logLevel
                      => Logging.VERBOSE_ ||
                         Logging.verboseExceptions_.Contains(logLevel)));

  private static readonly IList<LogLevel> verboseExceptions_ = new[] {
      LogLevel.Critical, LogLevel.Error, LogLevel.Warning,
  };


  public static void Initialize(bool verbose) {
    Logging.VERBOSE_ = verbose;
  }

  public static FinLogger Create<T>()
    => new Logger(Logging.FACTORY_.CreateLogger<T>());

  public static FinLogger Create(string categoryName)
    => new Logger(Logging.FACTORY_.CreateLogger(categoryName));


  private class Logger(MicrosoftLogger impl) : ILogger {
    public IDisposable BeginScope(string scope)
      => Asserts.CastNonnull(impl.BeginScope(scope));

    public void LogInformation(string message)
      => impl.LogInformation(message);

    public void LogWarning(string message)
      => impl.LogWarning(message);

    public void LogError(string message)
      => impl.LogError(message);
  }
}