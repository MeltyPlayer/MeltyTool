using schema.binary;
using schema.binary.attributes;

namespace dat.schema.melee;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Melee/Pl/SBM_PlayerModelLookupTables.cs#L66
/// </summary>
[BinarySchema]
public partial class MeleeLookupEntry : IBinaryDeserializable {
  public uint Count { get; private set; }
  public uint ByteEntriesOffset { get; set; }

  [RSequenceLengthSource(nameof(Count))]
  [RAtPositionOrNull(nameof(ByteEntriesOffset))]
  public byte[] ByteEntries { get; set; }
}