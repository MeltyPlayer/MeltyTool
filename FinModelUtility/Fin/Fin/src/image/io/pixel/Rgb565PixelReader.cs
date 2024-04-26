using System;

using fin.image.formats;
using fin.util.color;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel {
  /// <summary>
  ///   Helper class for reading 16-bit RGB pixels, where the red channel has 5
  ///   bits, the green channel has 6 bits, and the blue channel has 5 bits.
  /// </summary>
  public class Rgb565PixelReader : IPixelReader<Rgb24> {
    public IImage<Rgb24> CreateImage(int width, int height)
      => new Rgb24Image(PixelFormat.RGB565, width, height);

    public void Decode(IBinaryReader br, Span<Rgb24> scan0, int offset) {
      var value = br.ReadUInt16();
      ColorUtil.SplitRgb565(value, out var r, out var g, out var b);
      scan0[offset] = new Rgb24(r, g, b);
    }
  }
}