using System;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats;

public class Rgba32Image : BImage<Rgba32> {
  public Rgba32Image(int width, int height) : this(
      PixelFormat.RGBA8888,
      width,
      height) { }

  public Rgba32Image(PixelFormat format, int width, int height) : this(
      format,
      new Image<Rgba32>(FinImage.ImageSharpConfig, width, height)) { }

  internal Rgba32Image(PixelFormat format, Image<Rgba32> impl) : base(
      format) {
    this.Impl = impl;
  }

  protected override Image<Rgba32> Impl { get; }

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
            r = pixel.R;
            g = pixel.G;
            b = pixel.B;
            a = pixel.A;
          }

          accessHandler(InternalGetHandler);
        });

  public override bool HasAlphaChannel => true;

  public void GetRgba32Bytes(Span<Rgba32> bytes)
    => this.Impl.CopyPixelDataTo(bytes);
}