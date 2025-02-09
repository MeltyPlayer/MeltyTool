using System;
using System.Collections.Concurrent;

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
  private static readonly ConcurrentDictionary<IReadOnlyImage, Bitmap>
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
    var stride = 4 * image.Width;

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
        var data = new Rgba32[image.Width * image.Height];
        BlitFinImageIntoArray_(image, data, 0, 0, image.Width);

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

  public static unsafe Bitmap AsMergedMipmapAvaloniaImage(
      this IReadOnlyTexture texture) {
    var mipmapImages = texture.MipmapImages;
    if (mipmapImages.Length == 1) {
      return texture.AsAvaloniaImage();
    }

    var firstImage = texture.Image;
    var pixelSize = new PixelSize((int) (firstImage.Width * 1.5f),
                                  firstImage.Height);
    var dpi = new Vector(96, 96);
    var stride = 4 * pixelSize.Width;
    var data = new Rgba32[pixelSize.Width * pixelSize.Height];

    var baseDstY = 0;
    for (var i = 0; i < mipmapImages.Length; i++) {
      var baseDstX = i == 0 ? 0 : firstImage.Width;

      var mipmapImage = mipmapImages[i];
      BlitFinImageIntoArray_(mipmapImage, data, baseDstX, baseDstY, pixelSize.Width);

      if (i >= 1) {
        baseDstY += mipmapImage.Height;
      }
    }

    fixed (Rgba32* ptr = data) {
      return new Bitmap(PixelFormat.Rgba8888,
                        AlphaFormat.Premul,
                        new IntPtr(ptr),
                        pixelSize,
                        dpi,
                        stride);
    }
  }

  private static void BlitFinImageIntoArray_(IReadOnlyImage src,
                                             Memory<Rgba32> dstMemory,
                                             int baseDstX,
                                             int baseDstY,
                                             int dstStride) {
    var width = src.Width;
    var height = src.Height;

    switch (src) {
      case Rgba32Image rgba32Image: {
        var dstSpan = dstMemory.Span;
        var dstX = baseDstX;

        using var fastLock = rgba32Image.Lock();
        var srcSpan = fastLock.Pixels;
        for (var y = 0; y < height; ++y) {
          var dstY = baseDstY + y;
          var dstRowSpan = dstSpan.Slice(dstY * dstStride + dstX, width);
          srcSpan.Slice(y * width, width).CopyTo(dstRowSpan);
        }

        break;
      }
      default: {
        src.Access(get => {
          var dstSpan = dstMemory.Span;
          for (var y = 0; y < height; ++y) {
            for (var x = 0; x < width; ++x) {
              get(x, y, out var r, out var g, out var b, out var a);

              var dstX = baseDstX + x;
              var dstY = baseDstY + y;
              dstSpan[dstY * dstStride + dstX] = new Rgba32(r, g, b, a);
            }
          }
        });
        break;
      }
    }
  }
}