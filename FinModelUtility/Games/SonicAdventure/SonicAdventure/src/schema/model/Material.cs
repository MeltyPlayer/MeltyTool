using fin.schema.color;

using schema.binary;

namespace sonicadventure.schema.model;

[Flags]
public enum MaterialFlags : uint;

/// <summary>
///   Shamelessly stolen from:
///   https://info.sonicretro.org/SCHG:Sonic_Adventure/Model_Format
/// </summary>
[BinarySchema]
public partial class Material : IBinaryConvertible {
  public Argb32 DiffuseColor { get; set; }
  public Argb32 SpecularColor { get; set; }
  public float Exponent { get; set; }
  public uint TextureId { get; set; }
  public MaterialFlags Flags { get; set; }
}