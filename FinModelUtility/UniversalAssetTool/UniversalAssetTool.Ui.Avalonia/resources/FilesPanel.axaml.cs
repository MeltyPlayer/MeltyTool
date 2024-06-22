using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.importers;
using fin.io;
using fin.util.linq;
using fin.util.strings;

using uni.ui.avalonia.resources.model;

namespace uni.ui.avalonia.resources;

public class FilesPanelViewModelForDesigner() {
  public IReadOnlyList<string> Paths { get; } = [
      "//foo/bar/file.mod",
      "//foo/bar/some-very-long-path-that-cannot-be-fully-shown.mod",
      "C:/foo/bar/file.mod",
      "C:/foo/bar/some-very-long-path-that-cannot-be-fully-shown.mod",
  ];
}

public class FilesPanelViewModel {
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

  public IReadOnlyList<string> Paths { get; }
}

public partial class FilesPanel : UserControl {
  public FilesPanel() {
    InitializeComponent();
  }
}