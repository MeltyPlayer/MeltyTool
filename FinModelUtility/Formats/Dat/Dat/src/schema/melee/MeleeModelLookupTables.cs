using schema.binary;
using schema.binary.attributes;

namespace dat.schema.melee;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Melee/Pl/SBM_PlayerModelLookupTables.cs#L5
/// </summary>
[BinarySchema]
public partial class MeleeModelLookupTables : IBinaryDeserializable {
  public uint VisibilityLookupLength { get; set; }
  public uint CostumeVisibilityLookupTableOffset { get; set; }

  // TODO: Handle rest

  [RAtPositionOrNull(nameof(CostumeVisibilityLookupTableOffset))]
  public MeleeCostumeLookupTable? CostumeVisibilityLookupTable { get; set; }
}