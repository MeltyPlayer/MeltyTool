using System;
using System.IO;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using fin.image;

namespace marioartisttool.util;

public static class AssetLoaderUtil {
  public static Stream Open(string assetPath)
    => AssetLoader.Open(
        new Uri(Path.Join("avares://MarioArtistTool/Assets", assetPath)));

  public static Bitmap LoadBitmap(string imagePath) {
    using var s = Open(imagePath);
    return new Bitmap(s);
  }

  public static IImage LoadImage(string imagePath) {
    using var s = Open(imagePath);
    return FinImage.FromStream(s);
  }
}