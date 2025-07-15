using fin.image;
using fin.image.io;
using fin.image.io.pixel;

using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema;

[BinarySchema]
public partial class L8Image(int width, int height) : IBinaryDeserializable {
  [Skip]
  private int Length => width * height;

  [RSequenceLengthSource(nameof(this.Length))]
  public byte[] Data { get; private set; }

  public IImage ToImage()
    => PixelImageReader
       .New(width, height, new L8PixelReader())
       .ReadImage(this.Data, Endianness.BigEndian);
}