using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.skl;

[BinarySchema]
public partial class Skl : IBinaryConvertible {
  private uint boneCount_;

  // M-1: Only value found is "2", possibly "IsTranslateAnimationEnabled"
  // flag (I can't find a change in-game)
  [Unknown]
  public uint unkFlags;

  [RSequenceLengthSource(nameof(boneCount_))]
  public Bone[] bones { get; set; }
}