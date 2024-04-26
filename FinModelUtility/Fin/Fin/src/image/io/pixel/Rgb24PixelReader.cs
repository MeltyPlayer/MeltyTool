using System;

using fin.color;
using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel {
  /// <summary>
  ///   Helper class for reading 24-bit RGB pixels.
  /// </summary>
  public class Rgb24PixelReader : IPixelReader<Rgb24> {
    public IImage<Rgb24> CreateImage(int width, int height)
      => new Rgb24Image(PixelFormat.RGB888, width, height);

    public void Decode(IBinaryReader br, Span<Rgb24> scan0, int offset) {
      FinColor.SplitRgb(br.ReadInt24(), out var r, out var g, out var b);
      scan0[offset] = new Rgb24(r, g, b);
    }
  }
}