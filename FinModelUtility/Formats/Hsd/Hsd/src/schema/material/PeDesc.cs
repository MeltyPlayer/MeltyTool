using gx;

using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema.material;

[Flags]
public enum PIXEL_PROCESS_ENABLE : byte {
  COLOR_UPDATE = (1 << 0),
  ALPHA_UPDATE = (1 << 1),
  DST_ALPHA = (1 << 2),
  BEFORE_TEX = (1 << 3),
  COMPARE = (1 << 4),
  ZUPDATE = (1 << 5),
  DITHER = (1 << 6)
}

[BinarySchema]
public partial class PeDesc : IBinaryDeserializable {
  public PIXEL_PROCESS_ENABLE Flags { get; set; }

  [NumberFormat(SchemaNumberType.UN8)]
  public float AlphaRef0 { get; set; }

  [NumberFormat(SchemaNumberType.UN8)]
  public float AlphaRef1 { get; set; }

  [NumberFormat(SchemaNumberType.UN8)]
  public float DestinationAlpha { get; set; }

  public GxBlendMode BlendMode { get; set; }
  public GxBlendFactor SrcFactor { get; set; }
  public GxBlendFactor DstFactor { get; set; }
  public GxLogicOp BlendOp { get; set; }
  public GxCompareType DepthFunction { get; set; }
  public GxCompareType AlphaComp0 { get; set; }
  public GxAlphaOp AlphaOp { get; set; }
  public GxCompareType AlphaComp1 { get; set; }
}