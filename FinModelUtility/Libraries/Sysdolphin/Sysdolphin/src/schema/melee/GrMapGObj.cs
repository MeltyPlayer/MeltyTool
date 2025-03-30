using schema.binary;
using schema.binary.attributes;

using sysdolphin.schema.animation;

namespace sysdolphin.schema.melee;

[BinarySchema]
public partial class GrMapGObj : IBinaryDeserializable {
  public uint RootJObjOffset { get; set; }
  public uint JointAnimationsOffset { get; set; }
  public uint MaterialAnimationsOffset { get; set; }
  public uint ShapeAnimationsOffset { get; set; }

  [SequenceLengthSource(0x24)]
  public byte[] Unk { get; set; }

  [RAtPositionOrNull(nameof(RootJObjOffset))]
  public JObj? RootJObj { get; private set; }

  [RAtPositionOrNull(nameof(JointAnimationsOffset))]
  public HsdNullTerminatedPointerArray<AnimJoint>? JointAnimations { get; private set; }
}