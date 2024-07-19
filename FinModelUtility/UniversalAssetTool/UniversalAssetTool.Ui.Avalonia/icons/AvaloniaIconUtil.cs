using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using fin.image;
using fin.image.formats;
using fin.model;

using SixLabors.ImageSharp.PixelFormats;

using PixelFormat = Avalonia.Platform.PixelFormat;

namespace uni.ui.avalonia.icons;

public static class AvaloniaIconUtil {
  private static readonly Dictionary<IReadOnlyImage, Bitmap>
      imageCache_ = new();

  public static void ClearCache() => imageCache_.Clear();

  public static string GetUri(string imageName)
    => $"avares://UniversalAssetTool.Ui.Avalonia/icons/{imageName}.png";

  public static Bitmap Load(string imageName)
    => new(AssetLoader.Open(new Uri(GetUri(imageName))));

  public static Bitmap AsAvaloniaImage(this IReadOnlyTexture texture)
    => AsAvaloniaImage(texture.Image);

  public static unsafe Bitmap AsAvaloniaImage(this IReadOnlyImage image) {
    if (imageCache_.TryGetValue(image, out var bitmap)) {
      return bitmap;
    }

    var pixelSize = new PixelSize(image.Width, image.Height);
    var dpi = new Vector(96, 96);
    var stride = image.Width;

    switch (image) {
      case Rgba32Image rgba32Image: {
        using var fastLock = rgba32Image.Lock();
        fixed (Rgba32* ptr = &fastLock.Pixels.GetPinnableReference()) {
          bitmap = new Bitmap(PixelFormat.Rgba8888,
                              AlphaFormat.Unpremul,
                              new IntPtr(ptr),
                              pixelSize,
                              dpi,
                              stride);
        }

        break;
      }
      default: {
        var data = new Rgba32[4 * image.Width * image.Height];
        image.Access(get => {
          for (var y = 0; y < image.Height; ++y) {
            for (var x = 0; x < image.Width; ++x) {
              get(x, y, out var r, out var g, out var b, out var a);
              data[4 * (y * image.Width + x)] = new Rgba32(r, g, b, a);
            }
          }
        });

        fixed (Rgba32* ptr = data) {
          bitmap = new Bitmap(PixelFormat.Rgba8888,
                              AlphaFormat.Premul,
                              new IntPtr(ptr),
                              pixelSize,
                              dpi,
                              stride);
        }

        break;
      }
    }

    return imageCache_[image] = bitmap;
  }
}