using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.luts;

[BinarySchema]
public partial class LutSet : IBinaryConvertible {
  public ushort BitFlags; //Not sure

  [WLengthOfSequence(nameof(Keyframes))]
  private ushort keyframeCount_;

  public short Start;
  public short End;

  [RSequenceLengthSource(nameof(keyframeCount_))]
  public LutKeyframe[] Keyframes;

  [Unknown]
  public float unk1;

  [Unknown]
  public float unk2;
}