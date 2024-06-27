using System;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats;

public class La16Image : BImage<La16> {
  public La16Image(PixelFormat format, int width, int height) : this(
      format,
      new Image<La16>(FinImage.ImageSharpConfig, width, height)) { }

  internal La16Image(PixelFormat format, Image<La16> impl) : base(
      format) {
    this.Impl = impl;
  }

  protected override Image<La16> Impl { get; }

  public override void Access(IImage.AccessHandler accessHandler) {
    var frame = this.Impl.Frames[0];

    void GetHandler(
        int x,
        int y,
        out byte r,
        out byte g,
        out byte b,
        out byte a) {
      var pixel = frame[x, y];
      r = g = b = pixel.L;
      a = pixel.A;
    }

    accessHandler(GetHandler);
  }

  public override bool HasAlphaChannel => true;

  public void GetIa16Bytes(Span<La16> dst)
    => this.Impl.CopyPixelDataTo(dst);
}