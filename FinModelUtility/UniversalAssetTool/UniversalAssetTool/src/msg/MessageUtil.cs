using fin.io;
using fin.log;

namespace uni.msg;

public static class MessageUtil {
  public static void LogExporting<T>(
      ILogger logger,
      IFileHierarchyDirectory directory,
      IReadOnlyList<T> rawModels) {
      if (rawModels.Count == 1) {
        LogExporting(logger, rawModels[0]);
      } else {
        logger.LogInformation(
            $"Exporting models from {directory.LocalPath}");
      }
    }

  public static void LogExporting<T>(
      ILogger logger,
      T rawModel)
    => logger.LogInformation(
        $"Exporting model {rawModel}");

  public static void LogAlreadyProcessed<T>(
      ILogger logger,
      IFileHierarchyDirectory directory,
      IReadOnlyList<T> rawModels) {
      if (rawModels.Count == 1) {
        LogAlreadyProcessed(logger, rawModels[0]);
      } else {
        logger.LogInformation(
            $"Already processed models from {directory.LocalPath}");
      }
    }

  public static void LogAlreadyProcessed<T>(
      ILogger logger,
      T rawModel) {
      logger.LogInformation(
          $"Already processed model {rawModel}");
    }
}