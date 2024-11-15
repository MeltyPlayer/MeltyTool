using System;

using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 4-bit luminance pixels.
/// </summary>
public class A4PixelReader : IPixelReader<La16> {
  public IImage<La16> CreateImage(int width, int height)
    => new La16Image(PixelFormat.A4, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<La16> scan0, int offset) {
    var value = data[0];

    var upper = (byte) ((value >> 4) * 17);
    var lower = (byte) ((value & 0xF) * 17);

    scan0[offset + 0] = new La16(0xFF, upper);
    scan0[offset + 1] = new La16(0xFF, lower);
  }

  public int PixelsPerRead => 2;
  public int BitsPerPixel => 4;
}