using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using fin.color;

namespace fin.image.formats;

public abstract class BIndexedImage : IImage {
  private readonly IImage impl_;

  protected BIndexedImage(PixelFormat pixelFormat,
                          IImage impl,
                          IColor[] palette) {
    this.PixelFormat = pixelFormat;
    this.impl_ = impl;
    this.Palette = palette;
  }

  ~BIndexedImage() => this.Dispose();

  public void Dispose() {
    this.impl_.Dispose();
    GC.SuppressFinalize(this);
  }

  public IColor[] Palette { get; }
  public PixelFormat PixelFormat { get; }
  public int Width => this.impl_.Width;
  public int Height => this.impl_.Height;

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