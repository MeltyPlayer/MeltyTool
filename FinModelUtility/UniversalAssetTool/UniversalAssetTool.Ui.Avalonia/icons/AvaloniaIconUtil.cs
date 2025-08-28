using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace uni.ui.avalonia.icons;

public static class AvaloniaIconUtil {
  public static string GetUri(string imageName)
    => $"avares://UniversalAssetTool.Ui.Avalonia/icons/{imageName}.png";

  public static Bitmap Load(string imageName)
    => new(AssetLoader.Open(new Uri(GetUri(imageName))));
}