using Avalonia.Controls;

using fin.util.asserts;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources;

internal static class FilesPanelExtensions {
  extension(FilesPanel filesPanel) {
    public IEnumerable<string> GetAllPaths()
      => filesPanel.Single<DataGrid>()
                   .All<TextBlock>()
                   .Select(t => t.Text.AssertNonnull());
  }
}