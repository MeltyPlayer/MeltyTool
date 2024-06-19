using System;
using System.IO;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using fin.image;
using fin.model;

namespace uni.ui.avalonia.icons;

public static class AvaloniaIconUtil {
  public static string GetUri(string imageName)
    => $"avares://UniversalAssetTool.Ui.Avalonia/icons/{imageName}.png";

  public static Bitmap Load(string imageName)
    => new(AssetLoader.Open(new Uri(GetUri(imageName))));

  public static Bitmap AsAvaloniaImage(this IReadOnlyImage image) {
    using var ms = new MemoryStream();
    image.ExportToStream(ms, LocalImageFormat.PNG);
    ms.Flush();
    ms.Position = 0;
    return new Bitmap(ms);
  }

  public static Bitmap AsAvaloniaImage(this IReadOnlyTexture texture) {
    using var ms = new MemoryStream();
    texture.WriteToStream(ms);
    ms.Flush();
    ms.Position = 0;
    return new Bitmap(ms);
  }
}