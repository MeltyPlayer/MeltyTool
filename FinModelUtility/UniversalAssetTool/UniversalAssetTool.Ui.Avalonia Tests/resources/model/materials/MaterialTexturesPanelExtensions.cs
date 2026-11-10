using Avalonia.Controls;

using fin.util.asserts;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources.model.materials;

internal static class MaterialTexturesPanelExtensions {
  extension(MaterialTexturesPanel materialTexturesPanel) {
    public string GetSelectedTextureName()
      => materialTexturesPanel
         .First<StackPanel>()
         .Single<SelectableTextBlock>()
         .Text.AssertNonnull();
  }
}