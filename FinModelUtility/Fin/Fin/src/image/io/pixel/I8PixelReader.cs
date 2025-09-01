using System;

using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 8-bit intensity pixels.
/// </summary>
public class I8PixelReader : IPixelReader<La16> {
  public IImage<La16> CreateImage(int width, int height)
    => new I8Image(PixelFormat.I8, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<La16> scan0, int offset) {
    var value = data[0];
    scan0[offset] = new La16(value, value);
  }

  public int BitsPerPixel => 8;
}