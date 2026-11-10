using System;

using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 8-bit intensity pixels.
/// </summary>
public sealed class I8PixelReader : IPixelReader<L8> {
  public IImage<L8> CreateImage(int width, int height)
    => new I8Image(PixelFormat.I8, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<L8> scan0, int offset) {
    var value = data[0];
    scan0[offset] = new L8(value);
  }

  public int BitsPerPixel => 8;
}