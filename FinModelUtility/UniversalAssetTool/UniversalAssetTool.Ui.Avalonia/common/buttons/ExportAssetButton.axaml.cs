using Avalonia.Controls;
using Avalonia.Interactivity;

namespace uni.ui.avalonia.common.buttons;

public partial class ExportAssetButton : UserControl {
  public ExportAssetButton() => this.InitializeComponent();

  private async void Button_OnClick(object? sender, RoutedEventArgs e) { }

  /*private void StartExportingModelsInBackground_(
      IReadOnlyList<IAnnotatedFileBundle<IModelFileBundle>>
          modelFileBundles) {
    var extractorPromptChoice =
        PromptIfModelFileBundlesAlreadyExported_(
            modelFileBundles,
            Config.Instance.Exporter.General.ExportedFormats);
    if (extractorPromptChoice != ExporterPromptChoice.CANCEL) {
      this.CancellationToken = new CancellationTokenSource();

      Task.Run(() => {
        ExportAll(modelFileBundles,
                  new GlobalModelImporter(),
                  this.Progress,
                  this.CancellationToken,
                  Config.Instance.Exporter.General
                        .ExportedFormats,
                  extractorPromptChoice ==
                  ExporterPromptChoice
                      .OVERWRITE_EXISTING);
      });
    }
  }

  private string GetTotalNodeText_(IFileTreeNode node) {
    var totalText = "";
    var directory = node;
    while (true) {
      if (totalText.Length > 0) {
        totalText = "/" + totalText;
      }

      totalText = directory.Text + totalText;

      directory = directory.Parent;
      if (directory?.Parent == null) {
        break;
      }
    }

    return totalText;
  }

  private static async Task<ExporterPromptChoice>
      PromptIfModelFileBundlesAlreadyExported_(
          IReadOnlyList<IAnnotatedFileBundle> modelFileBundles,
          IReadOnlySet<ExportedFormat> formats) {
    if (CheckIfModelFileBundlesAlreadyExported(
            modelFileBundles,
            formats,
            out var existingOutputFiles)) {
      var totalCount = modelFileBundles.Count;
      if (totalCount == 1) {
        var result = await MessageBoxManager
                     .GetMessageBoxStandard(
                         "Model has already been exported!",
                         $"Model defined in \"{existingOutputFiles.First().FileBundle.DisplayFullPath}\" has already been exported. Would you like to overwrite it?",
                         ButtonEnum.YesNo,
                         Icon.Warning
                     ).ShowAsync();
        return result switch {
            ButtonResult.Yes => ExporterPromptChoice.OVERWRITE_EXISTING,
            ButtonResult.No  => ExporterPromptChoice.CANCEL,
        };
      } else {
        var existingCount = existingOutputFiles.Count();
        var result = await MessageBoxManager
                           .GetMessageBoxStandard(
                               $"{existingCount}/{totalCount} models have already been exported!",
                               $"{existingCount} model{(existingCount != 1 ? "s have" : " has")} already been exported. Select 'Yes' to overwrite them, 'No' to skip them, or 'Cancel' to abort this operation.",
                               ButtonEnum.YesNoCancel,
                               Icon.Warning
                           ).ShowAsync();
        return result switch {
            ButtonResult.Yes    => ExporterPromptChoice.OVERWRITE_EXISTING,
            ButtonResult.No     => ExporterPromptChoice.SKIP_EXISTING,
            ButtonResult.Cancel => ExporterPromptChoice.CANCEL,
        };
      }
    }

    return ExporterPromptChoice.SKIP_EXISTING;
  }*/
}