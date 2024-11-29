using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats;

public class L8Image : BImage<L8> {
  public L8Image(PixelFormat format, int width, int height) : this(
      format,
      new Image<L8>(FinImage.ImageSharpConfig, width, height)) { }

  internal L8Image(PixelFormat format, Image<L8> impl) : base(
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
            r = g = b = pixel.PackedValue;
            a = 255;
          }

          accessHandler(InternalGetHandler);
        });

  public override bool HasAlphaChannel => false;
}