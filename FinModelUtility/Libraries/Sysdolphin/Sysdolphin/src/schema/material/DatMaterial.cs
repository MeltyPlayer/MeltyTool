using fin.schema.color;

using schema.binary;

namespace sysdolphin.schema.material;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/93a906444f34951c6eed4d8c6172bba43d4ada98/HSDRaw/Common/HSD_MOBJ.cs#L101
/// </summary>
[BinarySchema]
public partial class DatMaterial : IBinaryDeserializable {
  public Rgba32 AmbientColor { get; set; }
  public Rgba32 DiffuseColor { get; set; }
  public Rgba32 SpecularColor { get; set; }
  public float Alpha { get; set; }
  public float Shininess { get; set; }
}