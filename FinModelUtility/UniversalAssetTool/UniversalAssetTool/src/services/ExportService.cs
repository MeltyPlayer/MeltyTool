using System.Reactive.Subjects;
using System.Text;

using fin.io.bundles;
using fin.io.web;
using fin.math.floats;
using fin.model.io;
using fin.util.progress;
using fin.util.tasks;
using fin.util.types;

using uni.api;
using uni.config;
using uni.games;
using uni.ui.winforms.common.fileTreeView;

namespace uni.services;

public enum DialogResult {
  CANCEL,
  YES,
  NO,
}

[IocCandiate]
public static class ExportService {
  public static event Action OnExportStart;
  public static event Action OnExportComplete;

  public static MemoryProgress<(float, IModelFileBundle?)> Progress { get; } =
    new((0, null));

  public static CancellationTokenSource? CancellationToken { get; private set; }

  public static bool IsStarted => CancellationToken != null;

  private static readonly BehaviorSubject<bool>
      isInProjectSubject_ = new(false);

  public static IObservable<bool> ΔIsInProgress => isInProjectSubject_;

  public static bool IsInProgress
    => IsStarted && !Progress.Current.Item1.IsRoughly1();


  static ExportService() {
    Progress.ProgressChanged += (_, current) => {
      var (_, modelFileBundle) = current;
      if (IsInProgress && modelFileBundle != null) {
        AnnouncementService.DisplayAnnouncement(
            new Announcement(
                AnnouncementType.INFO,
                $"Exporting model to /out: {modelFileBundle.DisplayFullPath}") {
                CancellationTokenSource = CancellationToken
            });
      }
    };
  }

  public delegate Task<DialogResult> ShowMessageBox(
      string title,
      string message,
      bool includeCancel);

  public static async Task ExportAllModelsInDirectory(
      IFileTreeParentNode directoryNode,
      ShowMessageBox showMessageBox) {
    var models = directoryNode.GetFilesOfType<IModelFileBundle>(true)
                              .ToArray();
    await StartExportingModelsInBackground_(models, showMessageBox);
  }

  public static async Task ExportSelectedFile(
      IModelFileBundle modelFileBundle,
      ShowMessageBox showMessageBox)
    => await StartExportingModelsInBackground_(
        [modelFileBundle],
        showMessageBox);

  private static async Task StartExportingModelsInBackground_(
      IReadOnlyList<IModelFileBundle> modelFileBundles,
      ShowMessageBox showMessageBox) {
    var extractorPromptChoice =
        await PromptIfModelFileBundlesAlreadyExported_(
            modelFileBundles,
            showMessageBox,
            Config.Instance.Exporter.General.ExportedFormats);
    if (extractorPromptChoice != ExporterUtil.ExporterPromptChoice.CANCEL) {
      CancellationToken = new CancellationTokenSource();

      FinTask.Run(() => {
        isInProjectSubject_.OnNext(true);
        OnExportStart?.Invoke();

        var results = ExporterUtil.ExportAll(
            modelFileBundles,
            Progress,
            CancellationToken,
            Config.Instance.Exporter.General.ExportedFormats,
            extractorPromptChoice ==
            ExporterUtil.ExporterPromptChoice.OVERWRITE_EXISTING);

        var total = results.Length;
        var successCount = results.Count(r => r.result is ExportResult.SUCCESS);
        var skipCount
            = results.Count(r => r.result is ExportResult.SKIPPED
                                             or ExportResult.ALREADY_EXISTS);

        var messageSb = new StringBuilder();
        messageSb.Append(
            $"Exported {successCount}/{total} asset{(successCount != 1 ? "s" : "")} successfully");

        if (skipCount == 0) {
          messageSb.Append('.');
        } else {
          messageSb.Append($"; skipped {skipCount}.");
        }

        FinTask.RunOnUiThread(() => {
          AnnouncementService.DisplayAnnouncement(
              new Announcement(
                  AnnouncementType.INFO,
                  messageSb.ToString()) {
                  ExceptionsAndContexts
                      = results
                        .Where(r => r.exception != null)
                        .Select(r => (r.exception!,
                                      (IExceptionContext?)
                                      new
                                          ExportFileBundleExceptionContext(
                                              r.fileBundle)))
                        .ToArray()
              });
        });

        isInProjectSubject_.OnNext(false);
        OnExportComplete?.Invoke();
      });
    }
  }

  private static async Task<ExporterUtil.ExporterPromptChoice>
      PromptIfModelFileBundlesAlreadyExported_(
          IReadOnlyList<IFileBundle> modelFileBundles,
          ShowMessageBox showMessageBox,
          IReadOnlySet<ExportedFormat> formats) {
    if (ExporterUtil.CheckIfModelFileBundlesAlreadyExported(
            modelFileBundles,
            formats,
            out var existingOutputFiles)) {
      var totalCount = modelFileBundles.Count;
      if (totalCount == 1) {
        var result = await showMessageBox(
            "Model has already been exported!",
            $"Model defined in \"{existingOutputFiles.First().DisplayFullPath}\" has already been exported. Would you like to overwrite it?",
            false);
        return result switch {
            DialogResult.YES => ExporterUtil.ExporterPromptChoice
                                            .OVERWRITE_EXISTING,
            DialogResult.NO => ExporterUtil.ExporterPromptChoice.CANCEL,
        };
      } else {
        var existingCount = existingOutputFiles.Count();
        var result = await showMessageBox(
            $"{existingCount}/{totalCount} models have already been exported!",
            $"{existingCount} model{(existingCount != 1 ? "s have" : " has")} already been exported. Select 'Yes' to overwrite them, 'No' to skip them, or 'Cancel' to abort this operation.",
            true);
        return result switch {
            DialogResult.YES => ExporterUtil.ExporterPromptChoice
                                            .OVERWRITE_EXISTING,
            DialogResult.NO => ExporterUtil.ExporterPromptChoice.SKIP_EXISTING,
            DialogResult.CANCEL => ExporterUtil.ExporterPromptChoice.CANCEL,
        };
      }
    }

    return ExporterUtil.ExporterPromptChoice.SKIP_EXISTING;
  }
}