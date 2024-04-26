using System;

using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel {
  /// <summary>
  ///   Helper class for reading 4-bit luminance pixels to both luminance and
  ///   alpha channels.
  /// </summary>
  public class L2a4PixelReader : IPixelReader<La16> {
    public IImage<La16> CreateImage(int width, int height)
      => new La16Image(PixelFormat.L4, width, height);

    public void Decode(IBinaryReader br, Span<La16> scan0, int offset) {
      var value = br.ReadByte();

      var upper = (byte) ((value >> 4) * 17);
      var lower = (byte) ((value & 0xF) * 17);

      scan0[offset + 0] = new La16(upper, upper);
      scan0[offset + 1] = new La16(lower, lower);
    }

    public int PixelsPerRead => 2;
  }
}