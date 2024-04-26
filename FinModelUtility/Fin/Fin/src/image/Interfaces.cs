using System;
using System.Drawing;
using System.IO;

using schema.readOnly;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image {
  public enum LocalImageFormat {
    BMP,
    PNG,
    JPEG,
    GIF,
    TGA,
    WEBP,
  }


  public static class LocalImageFormatExtensions {
    public static string GetExtension(this LocalImageFormat format)
      => format switch {
          LocalImageFormat.BMP  => ".bmp",
          LocalImageFormat.PNG  => ".png",
          LocalImageFormat.JPEG => ".jpg",
          LocalImageFormat.GIF  => ".gif",
          LocalImageFormat.TGA  => ".tga",
          LocalImageFormat.WEBP => ".webp",
      };
  }


  [GenerateReadOnly]
  public partial interface IImage : IDisposable {
    PixelFormat PixelFormat { get; }
    int Width { get; }
    int Height { get; }

    delegate void Rgba32GetHandler(int x,
                                   int y,
                                   out byte r,
                                   out byte g,
                                   out byte b,
                                   out byte a);

    delegate void AccessHandler(Rgba32GetHandler getHandler);

    [Const]
    void Access(AccessHandler accessHandler);

    bool HasAlphaChannel { get; }

    [Const]
    Bitmap AsBitmap();

    [Const]
    void ExportToStream(Stream stream, LocalImageFormat imageFormat);
  }

  [GenerateReadOnly]
  public partial interface IImage<TPixel> : IImage
      where TPixel : unmanaged, IPixel<TPixel> {
    [Const]
    IImageLock<TPixel> Lock();

    FinUnsafeImageLock<TPixel> UnsafeLock();
  }

  [GenerateReadOnly]
  public partial interface IImageLock<TPixel> : IDisposable
      where TPixel : unmanaged, IPixel<TPixel> {
    Span<byte> Bytes { get; }
    Span<TPixel> Pixels { get; }
  }
}