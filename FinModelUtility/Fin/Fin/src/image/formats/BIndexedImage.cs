using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using fin.color;

namespace fin.image.formats;

public abstract class BIndexedImage(
    PixelFormat pixelFormat,
    IImage impl,
    IColor[] palette)
    : IImage {
  ~BIndexedImage() => this.Dispose();

  public void Dispose() {
    impl.Dispose();
    GC.SuppressFinalize(this);
  }

  public IColor[] Palette { get; } = palette;
  public PixelFormat PixelFormat { get; } = pixelFormat;
  public int Width => impl.Width;
  public int Height => impl.Height;

  public abstract void Access(IImage.AccessHandler accessHandler);

  public bool HasAlphaChannel =>
      this.Palette.Any(color => Math.Abs(color.Af - 1) > .0001);

  public Bitmap AsBitmap() => FinImage.ConvertToBitmap(this);

  public void ExportToStream(Stream stream, LocalImageFormat imageFormat)
    => this.AsBitmap()
           .Save(stream,
                 imageFormat switch {
                     LocalImageFormat.BMP  => ImageFormat.Bmp,
                     LocalImageFormat.PNG  => ImageFormat.Png,
                     LocalImageFormat.JPEG => ImageFormat.Jpeg,
                     LocalImageFormat.GIF  => ImageFormat.Gif,
                     LocalImageFormat.WEBP => ImageFormat.Webp,
                 });
}