using System;

using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 8-bit luminance pixels.
/// </summary>
public class L8PixelReader : IPixelReader<L8> {
  public IImage<L8> CreateImage(int width, int height)
    => new L8Image(PixelFormat.L8, width, height);

  public void Decode(IBinaryReader br, Span<L8> scan0, int offset)
    => scan0[offset] = new L8(br.ReadByte());
}