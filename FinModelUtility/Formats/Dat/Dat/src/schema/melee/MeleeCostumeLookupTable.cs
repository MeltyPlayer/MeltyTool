using schema.binary;
using schema.binary.attributes;

namespace dat.schema.melee;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Melee/Pl/SBM_PlayerModelLookupTables.cs#L28
/// </summary>
[BinarySchema]
public partial class MeleeCostumeLookupTable : IBinaryDeserializable {
  public uint HighPolyOffset { get; set; }
  public uint LowPolyOffset { get; set; }

  // TODO: Handle rest

  [RAtPositionOrNull(nameof(HighPolyOffset))]
  public MeleeLookupTable? HighPoly { get; set; }

  [RAtPositionOrNull(nameof(LowPolyOffset))]
  public MeleeLookupTable? LowPoly { get; set; }
}