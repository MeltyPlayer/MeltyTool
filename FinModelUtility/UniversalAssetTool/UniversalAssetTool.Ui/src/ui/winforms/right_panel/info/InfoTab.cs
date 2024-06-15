using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using fin.importers;
using fin.io;
using fin.util.linq;
using fin.util.strings;

namespace uni.ui.winforms.right_panel.info {
  public partial class InfoTab : UserControl {
    public InfoTab() {
      InitializeComponent();
    }

    public IResource? Resource {
      set {
        var items = this.filesListBox_.Items;
        items.Clear();

        if (value == null) {
          return;
        }

        IEnumerable<string> paths;
        if (value.Files.WhereIs<IReadOnlyGenericFile, IFileHierarchyFile>()
                 .TryGetFirst(out var fileHierarchyFile)) {
          var hierarchy = fileHierarchyFile.Hierarchy;

          paths
              = value
                .Files
                .Select(file => {
                          if (file.DisplayFullPath.TryRemoveStart(
                                  hierarchy.Root.FullPath,
                                  out var trimmed)) {
                            return $"//{hierarchy.Name}{trimmed.Replace('\\', '/')}";
                          } else {
                            return file.DisplayFullPath;
                          }
                        })
                .Distinct()
                .Order();
        } else {
          paths = value.Files.Select(file => file.DisplayFullPath)
                       .Distinct()
                       .Order();
        }

        foreach (var path in paths) {
          items.Add(path);
        }
      }
    }
  }
}