using fin.image;
using fin.image.io;
using fin.image.io.pixel;

using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema;

[BinarySchema]
public partial class L4Image(int width, int height) : IMarioArtistImage {
  [Skip]
  private int Length => width * height / 2;

  [RSequenceLengthSource(nameof(this.Length))]
  public byte[] Data { get; private set; }

  public IImage ToImage()
    => PixelImageReader
       .New(width, height, new L4PixelReader())
       .ReadImage(this.Data, Endianness.BigEndian);
}