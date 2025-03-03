using gx;

using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema.texture;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/93a906444f34951c6eed4d8c6172bba43d4ada98/HSDRaw/Common/HSD_TOBJ.cs#L391
/// </summary>
[BinarySchema]
public partial class TObjLod : IBinaryDeserializable {
  [IntegerFormat(SchemaIntegerType.UINT32)]
  public GX_MIN_TEXTURE_FILTER MinFilter { get; set; }

  public float Bias { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool BiasClamp { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool EnableEdgeLod { get; set; }

  // TODO: Support this enum: https://github.com/Ploaj/HSDLib/blob/93a906444f34951c6eed4d8c6172bba43d4ada98/HSDRaw/Common/HSD_TOBJ.cs#L399
  public uint Anisotropy { get; set; }
}