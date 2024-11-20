using System.Numerics;

using fin.schema.color;

using schema.binary;
using schema.binary.attributes;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloSprite : IBinaryConvertible {
  [StringLengthSource(16)]
  public string TextureFilename { get; set; }

  public Rgba32 Color { get; private set; } = new();

  public Vector3 SpritePosition { get; private set; }
  public Vector2 SpriteSize { get; private set;}
  public ushort SpriteFlags { get; set; }
}