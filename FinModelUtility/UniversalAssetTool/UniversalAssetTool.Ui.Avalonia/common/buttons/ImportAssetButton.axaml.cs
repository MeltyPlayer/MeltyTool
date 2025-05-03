using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using fin.common;
using fin.io;
using fin.model.processing;

using uni.cli;

namespace uni.ui.avalonia.common.buttons;

public partial class ImportAssetButton : UserControl {
  public ImportAssetButton() => this.InitializeComponent();

  private async void Button_OnClick(object? sender, RoutedEventArgs e) {
    var plugins = PluginUtil.Plugins;
    var supportedExtensions =
        plugins.SelectMany(plugin => plugin.FileExtensions).ToHashSet();

    var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
    if (storageProvider == null) {
      return;
    }

    var romsDirectory = await storageProvider.TryGetFolderFromPathAsync(
        DirectoryConstants.ROMS_DIRECTORY.FullPath);

    var selectedStorageFiles
        = await storageProvider
            .OpenFilePickerAsync(new FilePickerOpenOptions {
                SuggestedStartLocation = romsDirectory,
                AllowMultiple = true,
                Title = "Import single asset from files",
                FileTypeFilter = [
                    new FilePickerFileType(
                        "All supported plugin extensions") {
                        Patterns = supportedExtensions
                                   .Select(extension => $"*{extension}")
                                   .ToArray()
                    }
                ]
            });
    if (selectedStorageFiles.Count == 0) {
      return;
    }

    var inputFiles
        = selectedStorageFiles
          .Select(f => (IReadOnlySystemFile) new FinFile(f.Path.AbsolutePath))
          .ToArray();
    var bestMatch
        = plugins.FirstOrDefault(plugin => plugin.SupportsFiles(inputFiles));
    if (bestMatch == null) {
      // TODO: Show an error dialog
      return;
    }

    var finModel = bestMatch.ImportAndProcess(inputFiles);
    ModelService.OpenModel(null, finModel);
  }
}