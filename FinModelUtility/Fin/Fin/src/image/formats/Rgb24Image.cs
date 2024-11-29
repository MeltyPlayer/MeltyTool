using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats;

public class Rgb24Image : BImage<Rgb24> {
  public Rgb24Image(PixelFormat format, int width, int height) : this(
      format,
      new Image<Rgb24>(FinImage.ImageSharpConfig, width, height)) { }

  internal Rgb24Image(PixelFormat format, Image<Rgb24> impl) : base(
      format) {
    this.Impl = impl;
  }

  protected override Image<Rgb24> Impl { get; }

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
      r = pixel.R;
      g = pixel.G;
      b = pixel.B;
      a = 255;
    }

    accessHandler(GetHandler);
  }

  public override bool HasAlphaChannel => false;
}