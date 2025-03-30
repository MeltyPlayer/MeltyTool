using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema.melee;

[BinarySchema]
public partial class GrMapHead : IDatNode, IBinaryDeserializable {
  public uint GeneralPointsOffset { get; set; }
  public uint GeneralPointsCount { get; set; }

  public uint ModelGroupsOffset { get; set; }
  public uint ModelGroupsCount { get; set; }

  [RAtPositionOrNull(nameof(ModelGroupsOffset))]
  [RSequenceLengthSource(nameof(ModelGroupsCount))]
  public GrMapGObj[]? ModelGroups { get; set; }
}