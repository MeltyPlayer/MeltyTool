using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://kuribo64.net/get.php?id=KBNyhM0kmNiuUBb3
/// </summary>
[BinarySchema]
public partial class Material : IBinaryConvertible {
  private uint nameOffset_;

  [NullTerminatedString]
  [RAtPosition(nameof(nameOffset_))]
  public string Name { get; set; }

  public uint TextureId { get; set; }
  public uint TexturePaletteId { get; set; }

  public FixedPointVector2 TextureScale { get; set; }
  public uint TextureRotation { get; set; }
  public FixedPointVector2 TextureTranslation { get; set; }

  public uint TextureParameters { get; set; }
  public uint PolygonAttributes { get; set; }
  public uint DiffuseAmbientParameters { get; set; }
  public uint SpecularEmissionParameters { get; set; }
}