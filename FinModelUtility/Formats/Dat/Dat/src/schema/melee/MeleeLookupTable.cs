using schema.binary;
using schema.binary.attributes;

namespace dat.schema.melee;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Melee/Pl/SBM_PlayerModelLookupTables.cs#L41
/// </summary>
[BinarySchema]
public partial class MeleeLookupTable : IBinaryDeserializable {
  public uint Count { get; private set; }
  public uint LookupEntriesOffset { get; set; }

  [RSequenceLengthSource(nameof(Count))]
  [RAtPositionOrNull(nameof(LookupEntriesOffset))]
  public MeleeLookupEntry[]? LookupEntries { get; set; }
}