using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats;

public sealed class I8Image : BImage<L8> {
  public I8Image(PixelFormat format, int width, int height) : this(
      format,
      new Image<L8>(width, height)) { }

  internal I8Image(PixelFormat format, Image<L8> impl) : base(
      format) {
    this.Impl = impl;
  }

  protected override Image<L8> Impl { get; }

  public override void Access(IImage.AccessHandler accessHandler)
    => FinImage.Access(
        this.Impl,
        getHandler => {
          void InternalGetHandler(
              int x,
              int y,
              out byte r,
              out byte g,
              out byte b,
              out byte a) {
            getHandler(x, y, out var pixel);
            r = g = b = a = pixel.PackedValue;
          }

          accessHandler(InternalGetHandler);
        });

  public override bool HasAlphaChannel => true;

  public override void ExportToStream(Stream stream, LocalImageFormat imageFormat) {
    using var tmpLa16Image = new La16Image(this.PixelFormat, this.Width, this.Height);

    using var srcFastLock = this.Lock();
    using var dstFastLock = tmpLa16Image.Lock();
    var srcScan0 = srcFastLock.Pixels;
    var dstScan0 = dstFastLock.Pixels;
    for (var y = 0; y < this.Height; y++) {
      for (var x = 0; x < this.Width; x++) {
        var i = y * this.Width + x;
        var intensity = srcScan0[i].PackedValue;
        dstScan0[i] = new La16(intensity, intensity);
      }
    }

    tmpLa16Image.ExportToStream(stream, imageFormat);
  }
}