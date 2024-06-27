using System;

using fin.color;
using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 32-bit RGBA pixels.
/// </summary>
public class Rgba32PixelReader : IPixelReader<Rgba32> {
  public IImage<Rgba32> CreateImage(int width, int height)
    => new Rgba32Image(PixelFormat.RGBA8888, width, height);

  public void Decode(IBinaryReader br, Span<Rgba32> scan0, int offset) {
    FinColor.SplitRgba(br.ReadInt32(),
                       out var r,
                       out var g,
                       out var b,
                       out var a);
    scan0[offset] = new Rgba32(r, g, b, a);
  }
}