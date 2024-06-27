using fin.schema.color;

using schema.binary;

namespace grezzo.schema.cmb.mats;

[BinarySchema]
public partial class TexMapper : IBinaryConvertible {
  public short textureId { get; set; }
  private readonly ushort padding_ = 0;
  public TextureMinFilter minFilter;
  public TextureMagFilter magFilter;
  public TextureWrapMode wrapS;
  public TextureWrapMode wrapT;
  public float minLodBias;
  public float lodBias;
  public Rgba32 BorderColor { get; private set; } = new();
}