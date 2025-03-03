using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema;

/// <summary>
///   Scene object.
/// 
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/1a03d63a35376adc79a0a7495a389ea1a9dc4226/HSDRaw/Common/HSD_SOBJ.cs#L7
/// </summary>
[BinarySchema]
public partial class SObj : IDatNode, IBinaryDeserializable {
  public uint JObjDescOffset { get; set; }
  public uint CObjOffset { get; set; }
  public uint LObjOffset { get; set; }
  public uint FogAdjOffset { get; set; }

  [RAtPositionOrNull(nameof(JObjDescOffset))]
  public HsdNullTerminatedPointerArray<JObjDesc>? JObjDescs {
    get;
    private set;
  }
}