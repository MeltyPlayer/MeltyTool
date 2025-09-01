using fin.image;
using fin.image.io;
using fin.image.io.pixel;

using schema.binary;

namespace marioartist.schema;

public interface IMarioArtistImage : IBinaryDeserializable {
  IImage ToImage();
}

[BinarySchema]
public partial class Argb1555Image(int width, int height) : IMarioArtistImage {
  public byte[] Data { get; } = new byte[2 * width * height];

  public IImage ToImage()
    => PixelImageReader
       .New(width, height, new Argb1555PixelReader())
       .ReadImage(this.Data, Endianness.BigEndian);
}