using fin.model;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bmd;

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

  public int TextureId { get; set; }
  public int TexturePaletteId { get; set; }

  public FixedPointVector2 TextureScale { get; set; }
  public uint TextureRotation { get; set; }
  public FixedPointVector2 TextureTranslation { get; set; }

  public uint TextureParameters { get; set; }
  
  public uint PolygonAttributes { get; set; }

  [Skip]
  public CullingMode CullMode
    => (this.PolygonAttributes & 0xC0) switch {
        0x00 => CullingMode.SHOW_NEITHER,
        0x40 => CullingMode.SHOW_BACK_ONLY,
        0x80 => CullingMode.SHOW_FRONT_ONLY,
        0xC0 => CullingMode.SHOW_BOTH,
    };


  public uint DiffuseAmbientParameters { get; set; }
  public uint SpecularEmissionParameters { get; set; }
}