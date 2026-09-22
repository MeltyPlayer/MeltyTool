using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

using fin.common;
using fin.io;
using fin.model.io;
using fin.model.processing;
using fin.util.sets;
using fin.util.strings;
using fin.util.tasks;
using fin.util.types;

using uni.api;
using uni.config;
using uni.games;

namespace uni.ui.avalonia.services;

[IocCandiate]
public static class FileBundleExportService {
  private static IStorageFolder? lastDirectory_;
  private static string? lastExtension_;

  public static void ExportModelFileBundleTo(
      IModelFileBundle modelFileBundle,
      TopLevel? topLevel) {
    FinTask.Run(async () => await ExportModelFileBundleToAsync(
                    modelFileBundle,
                    topLevel));
  }

  public static async Task ExportModelFileBundleToAsync(
      IModelFileBundle modelFileBundle,
      TopLevel? topLevel) {
    var storageProvider = topLevel?.StorageProvider;
    if (storageProvider == null) {
      return;
    }

    var startLocation
        = lastDirectory_ ??
          await storageProvider.TryGetFolderFromPathAsync(
              DirectoryConstants.OUT_DIRECTORY.FullPath);
    var defaultExtension = lastExtension_ ?? ".fbx";

    var mainFile = modelFileBundle.MainFile;
    var suggestedFileName = mainFile != null
        ? $"{mainFile.NameWithoutExtension}{defaultExtension.ReplaceFirst('*', '.')}"
        : null;

    var selectedStorageFile
        = await storageProvider
            .SaveFilePickerAsync(new FilePickerSaveOptions {
                SuggestedStartLocation = startLocation,
                Title = "Export asset",
                DefaultExtension = defaultExtension,
                ShowOverwritePrompt = true,
                SuggestedFileName = suggestedFileName,
                FileTypeChoices =
                    ExporterUtil
                        .SupportedExportFormats
                        .OrderBy(f => f != defaultExtension.AsFormat())
                        .Select(f => new FilePickerFileType(f.GetName()) {
                            Patterns = [f.AsPattern()],
                        })
                        .ToArray()
            });
    if (selectedStorageFile == null) {
      return;
    }

    lastDirectory_ = await selectedStorageFile.GetParentAsync();

    var outputFile = new FinFile(selectedStorageFile.Path.AbsolutePath);
    var outputDirectory = outputFile.AssertGetParent();

    var outputFileType = outputFile.FileType;
    var outputExportFormat = outputFileType.AsFormat();
    lastExtension_ = outputExportFormat.AsFileExtension();

    ExporterUtil.Export(
        modelFileBundle,
        () => modelFileBundle.ImportAndProcess(),
        outputDirectory,
        outputExportFormat.AsSet(),
        true,
        outputFile.Name.ToString()
    );
  }
}