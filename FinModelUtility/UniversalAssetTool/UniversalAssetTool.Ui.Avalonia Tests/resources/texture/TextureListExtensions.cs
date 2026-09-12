using Avalonia.Controls;

using fin.util.asserts;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources.texture;

internal static class TextureListExtensions {
  extension(TextureList textureList) {
    public IEnumerable<string> GetTextureNames()
      => textureList
         .Single<ListBox>()
         .All<TextBlock>(t => t.Classes.Contains("regular"))
         .Select(t => t.Text.AssertNonnull());
  }
}