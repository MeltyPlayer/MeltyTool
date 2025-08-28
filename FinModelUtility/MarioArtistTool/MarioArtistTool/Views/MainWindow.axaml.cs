using System;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using fin.io;
using fin.io.web;
using fin.services;
using fin.ui.avalonia.dialogs;

using marioartist.api;
using marioartist.schema.mfs;

using marioartisttool.services;

using schema.binary;

namespace marioartisttool.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();

    ExceptionService.OnException += (e, c) => {
      Dispatcher.UIThread.Invoke(() => {
        var dialog = new ExceptionDialog {
            DataContext = new ExceptionDialogViewModel
                { Exception = e, Context = c },
            CanResize = false,
        };

        dialog.ShowDialog(this);
      });
    };

    Task.Run(this.AskUserForDiskFile_);
  }

  private async Task AskUserForDiskFile_() {
    var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
    if (storageProvider == null) {
      return;
    }

    var startLocation = await storageProvider.TryGetFolderFromPathAsync("./");

    var selectedStorageFiles
        = await storageProvider
            .OpenFilePickerAsync(new FilePickerOpenOptions {
                SuggestedStartLocation = startLocation,
                Title = "Select 64DD disk file",
                FileTypeFilter = [
                    new FilePickerFileType("All supported files") {
                        Patterns = [
                            "*.ndd", "*.ndr", "*.ram", "*.n64", "*.z64",
                            "*.disk"
                        ],
                    }
                ]
            });
    if (selectedStorageFiles is not { Count: 1 }) {
      return;
    }

    var selectedStorageFile = selectedStorageFiles[0];
    var diskFile = new FinFile(selectedStorageFile.Path.LocalPath);

    MfsDisk mfsDisk;
    try {
      using var br = diskFile.OpenReadAsBinary(Endianness.BigEndian);
      mfsDisk = br.ReadNew<MfsDisk>();
      var mfsRootDirectory = MfsTreeDirectory.CreateTreeFromMfsDisk(mfsDisk);
      MfsFileSystemService.LoadFileSystem(mfsRootDirectory);
    } catch (Exception e) {
      ExceptionService.HandleException(e, new LoadFileException(diskFile));
    }
  }
}