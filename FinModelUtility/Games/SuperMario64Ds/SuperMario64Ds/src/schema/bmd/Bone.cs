using fin.math;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bmd;

/// <summary>
///   Shamelessly stolen from:
///   https://kuribo64.net/get.php?id=KBNyhM0kmNiuUBb3
/// </summary>
[BinarySchema]
public partial class Bone : IBinaryConvertible {
  public uint Id { get; set; }

  private uint nameOffset_;

  [RAtPosition(nameof(nameOffset_))]
  [NullTerminatedString]
  public string Name { get; set; }

  public short OffsetToParentBone { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT16)]
  public bool HasChildren { get; set; }

  public int OffsetToNextSiblingBone { get; set; }

  public FixedPointVector3 Scale { get; set; }

  public EulerDegreesVector3 Rotation { get; set; }
  private readonly short padding_ = 0;

  public FixedPointVector3 Translation { get; set; }

  private uint displayListMaterialPairCount_;
  private uint materialIdsOffset_;
  private uint displayListIdsOffset_;

  public uint Parameters { get; set; }

  [Skip]
  public bool Billboard => this.Parameters.GetBit(0);

  [RSequenceLengthSource(nameof(displayListMaterialPairCount_))]
  [RAtPosition(nameof(materialIdsOffset_))]
  public byte[] MaterialIds { get; set; }

  [RSequenceLengthSource(nameof(displayListMaterialPairCount_))]
  [RAtPosition(nameof(displayListIdsOffset_))]
  public byte[] DisplayListIds { get; set; }
}