using System;

using CommunityToolkit.HighPerformance;

using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 16-bit luminance/alpha pixels.
/// </summary>
public class Al16PixelReader : IPixelReader<La16> {
  public IImage<La16> CreateImage(int width, int height)
    => new La16Image(PixelFormat.LA88, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<La16> scan0, int offset) {
    var la = data.Cast<byte, ushort>()[0];
    var l = (byte) (la & 0xFF);
    var a = (byte) (la >> 8);
    scan0[offset] = new La16(l, a);
  }

  public int BitsPerPixel => 16;
}