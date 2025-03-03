using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema.melee;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Melee/Pl/SBM_PlayerModelLookupTables.cs#L5
/// </summary>
[BinarySchema]
public partial class MeleeModelLookupTables : IBinaryDeserializable {
  public uint VisibilityLookupLength { get; set; }
  public uint CostumeVisibilityLookupTableOffset { get; set; }
  public uint MaterialLookupLength { get; set; }
  public uint CostumeMaterialLookupTableOffset { get; set; }
  public byte ItemHoldBone { get; set; }
  public byte ShieldBone { get; set; }
  public byte TopOfHeadBone { get; set; }
  public byte LeftFootBone { get; set; }
  public byte RightFootBone { get; set; }

  // TODO: Handle rest

  [RAtPositionOrNull(nameof(CostumeVisibilityLookupTableOffset))]
  public MeleeCostumeLookupTable? CostumeVisibilityLookupTable { get; set; }
}