using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using fin.common;
using fin.io;
using fin.io.bundles;
using fin.model.io;
using fin.util.sets;
using fin.util.strings;

using uni.api;
using uni.config;
using uni.games;

namespace uni.ui.avalonia.common.buttons;

public partial class ExportAssetButton : UserControl {
  public static readonly StyledProperty<IFileBundle?> FileBundleProperty =
      AvaloniaProperty.Register<ExportAssetButton, IFileBundle?>(
          nameof(FileBundle));

  public IFileBundle? FileBundle {
    get => this.GetValue(FileBundleProperty);
    set => this.SetValue(FileBundleProperty, value);
  }

  public ExportAssetButton() => this.InitializeComponent();


  protected void Button_OnClick(object? sender, RoutedEventArgs e) {
    if (this.FileBundle is IModelFileBundle modelFileBundle) {
      Task.Run(async () => await this.ExportModelFileBundle_(modelFileBundle));
    }
  }

  private IStorageFolder? lastDirectory_;
  private string? lastExtension_;

  private async Task ExportModelFileBundle_(
      IModelFileBundle modelFileBundle) {
    var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
    if (storageProvider == null) {
      return;
    }

    var startLocation
        = this.lastDirectory_ ??
          await storageProvider.TryGetFolderFromPathAsync(
              DirectoryConstants.OUT_DIRECTORY.FullPath);
    var defaultExtension = this.lastExtension_ ?? ".fbx";

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

    this.lastDirectory_ = await selectedStorageFile.GetParentAsync();

    var outputFile = new FinFile(selectedStorageFile.Path.AbsolutePath);
    var outputDirectory = outputFile.AssertGetParent();

    var outputFileType = outputFile.FileType;
    var outputExportFormat = outputFileType.AsFormat();
    this.lastExtension_ = outputExportFormat.AsFileExtension();

    ExporterUtil.Export(
        modelFileBundle,
        () => new GlobalModelImporter().Import(modelFileBundle),
        outputDirectory,
        outputExportFormat.AsSet(),
        true,
        outputFile.Name.ToString()
    );
  }
}