using System.Collections.Generic;
using System.IO;
using System.Linq;

using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using CommunityToolkit.Mvvm.ComponentModel;

using fin.config.avalonia.services;
using fin.importers;
using fin.io;
using fin.ui;
using fin.ui.avalonia.controls;
using fin.util.linq;
using fin.util.strings;

using ReactiveUI;

using uni.services;

namespace uni.ui.avalonia.resources;

public sealed class FilesPanelViewModelForDesigner : FilesPanelViewModel {
  public FilesPanelViewModelForDesigner() : base(null) {
    this.Paths = [
        "//foo/bar/file.mod",
        "//foo/bar/some-very-long-path-that-cannot-be-fully-shown.mod",
        "C:/foo/bar/file.mod",
        "C:/foo/bar/some-very-long-path-that-cannot-be-fully-shown.mod",
    ];
  }
}

public class FilesPanelViewModel : BViewModel {
  public FilesPanelViewModel(IResource? resource) {
    var files = resource?.Files;
    if (files == null) {
      return;
    }

    this.Files = files;

    IEnumerable<string> paths;
    if (resource.Files.WhereIs<IReadOnlyStandaloneFile, IFileHierarchyFile>()
                .TryGetFirst(out var fileHierarchyFile)) {
      var hierarchy = fileHierarchyFile.Hierarchy;

      paths = files
          .Select(file => {
            if (file.DisplayFullPath.TryRemoveStart(
                    hierarchy.Root.FullPath,
                    out var trimmed)) {
              return
                  $"//{hierarchy.Name}{trimmed.Replace('\\', '/')}";
            }

            return file.DisplayFullPath;
          });
    } else {
      paths = files.Select(file => file.DisplayFullPath);
    }

    this.Paths = [.. paths.Distinct().Order(StringUtil.NaturalSortInstance)];
  }

  public IReadOnlySet<IReadOnlyStandaloneFile> Files {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public IReadOnlyList<string> Paths {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class FilesPanel : BUserControl<FilesPanelViewModel> {
  public FilesPanel() {
    this.InitializeComponent();
  }

  private async void CopyAllToDirectory_(object? sender, RoutedEventArgs e) {
    var storageProvider = TopLevelService.Instance.StorageProvider;

    var selectedStorageFolders
        = await storageProvider
            .OpenFolderPickerAsync(new FolderPickerOpenOptions {
                Title = "Select directory to copy files to",
            });
    if (selectedStorageFolders is not { Count: 1 }) {
      return;
    }

    var selectedStorageFolder = selectedStorageFolders[0];
    var outputDirectory
        = new FinDirectory(selectedStorageFolder.Path.LocalPath);
    outputDirectory.Create();

    foreach (var file in this.ViewModel.Files) {
      var outputFile = new FinFile(
          Path.Join(outputDirectory.FullPath,
                    FinIoStatic.GetName(file.DisplayFullPath)));
      file.CopyTo(outputFile);
    }
  }
}