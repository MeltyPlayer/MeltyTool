using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.importers;
using fin.io;
using fin.util.linq;
using fin.util.strings;

using uni.ui.avalonia.model;

namespace uni.ui.avalonia.resource;

public class ResourcePanelViewModelForDesigner()
    : ResourcePanelViewModel(ModelDesignerUtil.CreateStubModel());

public class ResourcePanelViewModel {
  public ResourcePanelViewModel(IResource? resource) {
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

public partial class ResourcePanel : UserControl {
  public ResourcePanel() {
    InitializeComponent();
  }
}