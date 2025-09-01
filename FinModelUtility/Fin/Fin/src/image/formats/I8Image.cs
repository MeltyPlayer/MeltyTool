using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats;

public class I8Image : BImage<La16> {
  public I8Image(PixelFormat format, int width, int height) : this(
      format,
      new Image<La16>(FinImage.ImageSharpConfig, width, height)) { }

  internal I8Image(PixelFormat format, Image<La16> impl) : base(
      format) {
    this.Impl = impl;
  }

  protected override Image<La16> Impl { get; }

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
            r = g = b = a = pixel.L;
          }

          accessHandler(InternalGetHandler);
        });

  public override bool HasAlphaChannel => true;
}