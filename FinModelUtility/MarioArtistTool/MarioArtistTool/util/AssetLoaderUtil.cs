using System;
using System.IO;

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using fin.image;

namespace marioartisttool.util;

public static class AssetLoaderUtil {
  public static Stream Open(string assetPath)
    => AssetLoader.Open(
        new Uri(Path.Join("avares://MarioArtistTool/Assets", assetPath)));

  public static Bitmap LoadBitmap(string imagePath, int scale = 1) {
    using var s = Open(imagePath);

    var bitmap = new Bitmap(s);
    if (scale == 1) {
      return bitmap;
    }

    var size = bitmap.PixelSize;
    return bitmap.CreateScaledBitmap(
        new PixelSize(size.Width * scale, size.Height * scale),
        BitmapInterpolationMode.None);
  }

  public static IImage LoadImage(string imagePath) {
    using var s = Open(imagePath);
    return FinImage.FromStream(s);
  }
}