using System;

using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 8-bit luminance pixels to both luminance and
///   alpha channels.
/// </summary>
public class L2a8PixelReader : IPixelReader<La16> {
  public IImage<La16> CreateImage(int width, int height)
    => new La16Image(PixelFormat.L8, width, height);

  public void Decode(IBinaryReader br, Span<La16> scan0, int offset) {
    var value = br.ReadByte();
    scan0[offset] = new La16(value, value);
  }
}