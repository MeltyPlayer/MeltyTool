using System;

using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel {
  /// <summary>
  ///   Helper class for reading 16-bit luminance/alpha pixels.
  /// </summary>
  public class La16PixelReader : IPixelReader<La16> {
    public IImage<La16> CreateImage(int width, int height)
      => new La16Image(PixelFormat.LA88, width, height);

    public void Decode(IBinaryReader br, Span<La16> scan0, int offset) {
      var la = br.ReadUInt16();
      var a = (byte) (la & 0xFF);
      var l = (byte) (la >> 8);
      scan0[offset] = new La16(l, a);
    }
  }
}