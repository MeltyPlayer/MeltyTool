using System;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace uni.ui.avalonia.resources {
  public static class EmbeddedResourceUtil {
    public static string GetAvaloniaImageUri(string imageName)
      => $"avares://UniversalAssetTool.Ui.Avalonia/resources/images/{imageName}.png";

    public static Bitmap LoadAvaloniaImage(string imageName)
      => new(AssetLoader.Open(new Uri(GetAvaloniaImageUri(imageName))));
  }
}