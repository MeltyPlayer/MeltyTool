using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.importers;
using fin.io;
using fin.ui;
using fin.ui.avalonia.controls;
using fin.util.linq;
using fin.util.strings;

using ReactiveUI;

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

    IEnumerable<string> paths;
    if (resource.Files.WhereIs<IReadOnlyGenericFile, IFileHierarchyFile>()
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

    this.Paths = paths.Distinct().Order().ToArray();
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
}