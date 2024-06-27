using fin.schema.color;
using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloSprite : IBinaryConvertible {
  [StringLengthSource(16)]
  public string TextureFilename { get; set; }

  public Rgba32 Color { get; private set; } = new();

  public Vector3f SpritePosition { get; } = new();
  public Vector2f SpriteSize { get; } = new();
  public ushort SpriteFlags { get; set; }
}