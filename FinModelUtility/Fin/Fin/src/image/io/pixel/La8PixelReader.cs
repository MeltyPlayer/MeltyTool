using System;

using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;


namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 8-bit luminance/alpha pixels.
/// </summary>
public class La8PixelReader : IPixelReader<La16> {
  public IImage<La16> CreateImage(int width, int height)
    => new La16Image(PixelFormat.LA44, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<La16> scan0, int offset) {
    var value = data[0];

    var alpha = (byte) ((value >> 4) * 17);
    var luminance = (byte) ((value & 0xF) * 17);

    scan0[offset] = new La16(luminance, alpha);
  }

  public int BitsPerPixel => 8;
}