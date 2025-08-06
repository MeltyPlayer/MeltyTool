using fin.image;
using fin.image.io;
using fin.image.io.pixel;

using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema;

public interface IMarioArtistImage : IBinaryDeserializable {
  IImage ToImage();
}

[BinarySchema]
public partial class L8Image(int width, int height) : IMarioArtistImage {
  [Skip]
  private int Length => width * height;

  [RSequenceLengthSource(nameof(this.Length))]
  public byte[] Data { get; private set; }

  public IImage ToImage()
    => PixelImageReader
       .New(width, height, new L8PixelReader())
       .ReadImage(this.Data, Endianness.BigEndian);
}