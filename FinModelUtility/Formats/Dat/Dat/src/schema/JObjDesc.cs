using dat.schema.animation;

using schema.binary;
using schema.binary.attributes;

namespace dat.schema;

/// <summary>
///   Scene object.
/// 
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/1a03d63a35376adc79a0a7495a389ea1a9dc4226/HSDRaw/Common/HSD_SOBJ.cs#L7
/// </summary>
[BinarySchema]
public partial class JObjDesc : IBinaryDeserializable {
  public uint RootJObjOffset { get; set; }
  public uint JointAnimationsOffset { get; set; }
  public uint MaterialAnimationsOffset { get; set; }
  public uint ShapeAnimationsOffset { get; set; }

  [RAtPositionOrNull(nameof(RootJObjOffset))]
  public JObj? RootJObj { get; private set; }

  [RAtPositionOrNull(nameof(JointAnimationsOffset))]
  public HsdNullTerminatedPointerArray<AnimJoint>? JointAnimations { get; private set; }
}