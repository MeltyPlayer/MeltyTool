using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.model.blocks;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PistonMiner/ttyd-tools/blob/master/ttyd-tools/docs/MarioSt_AnimGroupBase.bt#L395
/// </summary>
[BinarySchema]
public partial class Animation : IBinaryConvertible {
  [StringLengthSource(16)]
  public string Name { get; set; }

  [SequenceLengthSource(0x40 - 16 - 4)]
  public byte[] Padding { get; set; }

  private uint dataOffset_;

  [RAtPositionOrNull(nameof(dataOffset_))]
  public AnimationData? Data { get; set; }
}