using modl.schema.modl.bw1.node;

using schema.binary;
using schema.binary.attributes;

namespace modl.schema.anim.bw1;

[BinarySchema]
public partial class Bw1AnimBone : IBwAnimBone, IBinaryConvertible {
  public string GetIdentifier() => Bw1Node.GetIdentifier(this.WeirdId);

  [StringLengthSource(16)] public string Name { get; set; }

  public uint PositionKeyframeCount { get; set; }
  public uint RotationKeyframeCount { get; set; }

  private readonly ulong padding0_ = 0;
  public float XPosDelta { get; set; }
  public float YPosDelta { get; set; }
  public float ZPosDelta { get; set; }
  public float XPosMin { get; set; }
  public float YPosMin { get; set; }
  public float ZPosMin { get; set; }
  private readonly uint padding1_ = 0;

  public uint WeirdId { get; set; }
}