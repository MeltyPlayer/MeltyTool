using System;

using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 4-bit intensity pixels.
/// </summary>
public class I4PixelReader : IPixelReader<La16> {
  public IImage<La16> CreateImage(int width, int height)
    => new I8Image(PixelFormat.I4, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<La16> scan0, int offset) {
    var value = data[0];

    var upper = (byte) ((value >> 4) * 17);
    var lower = (byte) ((value & 0xF) * 17);

    scan0[offset + 0] = new La16(upper, upper);
    scan0[offset + 1] = new La16(lower, lower);
  }

  public int PixelsPerRead => 2;
  public int BitsPerPixel => 4;
}