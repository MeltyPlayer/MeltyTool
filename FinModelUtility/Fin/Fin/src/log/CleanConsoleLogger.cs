using System;
using System.Collections.Concurrent;

using Crayon;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace fin.log;

using MicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using MicrosoftLoggerProvider = ILoggerProvider;

/// <summary>
///   From https://stackoverflow.com/questions/55924730/is-it-possible-to-disable-category-output-in-net-core-consolelogger-and-debuglo
/// </summary>
public class CleanConsoleLogger : MicrosoftLogger {
  public LogLevel LogLevel { get; set; } = LogLevel.Information;

  public IDisposable? BeginScope<TState>(TState state) => null;

  public bool IsEnabled(LogLevel logLevel) => logLevel >= this.LogLevel;

  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
    if (!this.IsEnabled(logLevel))
      return;

    string message = formatter(state, exception);
    if (exception != null)
      message += $"{Environment.NewLine}{exception}";

    switch (logLevel) {
      case LogLevel.Trace:
        Console.Out.WriteLineAsync(Output.Bright.Black(message));
        break;
      case LogLevel.Debug:
        Console.Out.WriteLineAsync(Output.Bright.Black(message));
        break;
      case LogLevel.Information:
        Console.Out.WriteLineAsync(message);
        break;
      case LogLevel.Warning:
        Console.Out.WriteLineAsync(Output.Dim().Yellow().Text(message));
        break;
      case LogLevel.Error:
        Console.Error.WriteLineAsync(Output.Bright.Red(message));
        break;
      case LogLevel.Critical:
        Console.Error.WriteLineAsync(Output.Bright.Red(message));
        break;
    }
  }
}

public class CleanConsoleLoggerProvider : MicrosoftLoggerProvider {
  private readonly ConcurrentDictionary<string, CleanConsoleLogger> _loggers = new ConcurrentDictionary<string, CleanConsoleLogger>();

  public MicrosoftLogger CreateLogger(string categoryName)
    => this._loggers.GetOrAdd(categoryName, name => new CleanConsoleLogger());

  public void Dispose() {
    this._loggers.Clear();
  }
}

public static class CleanConsoleLoggerFactoryExtensions {

  public static ILoggingBuilder AddCleanConsole(this ILoggingBuilder builder) {
    builder.Services.AddSingleton<MicrosoftLoggerProvider, CleanConsoleLoggerProvider>();
    return builder;
  }

}