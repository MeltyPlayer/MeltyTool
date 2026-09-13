using Avalonia.Controls;

using AvaloniaEdit;
using AvaloniaEdit.Document;

using fin.util.asserts;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources.model.materials;

internal static class MaterialShadersPanelExtensions {
  extension(MaterialShadersPanel materialShadersPanel) {
    public string GetDisplayedCode() {
      var tabItem = materialShadersPanel
          .Single<TabItem>(t => t.IsSelected);

      var tabContent = tabItem.Content.AssertAsA<Control>();

      return tabContent
             .Single<TextEditor>()
             .Text;
    }
  }
}