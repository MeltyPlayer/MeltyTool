using schema.binary;
using schema.binary.attributes;

namespace dat.schema.melee;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Melee/Pl/SBM_FighterData.cs
/// </summary>
[BinarySchema]
public partial class MeleeFighterData : IDatNode, IBinaryDeserializable {
  public uint Attributes { get; set; }
  public uint Attributes2 { get; set; }
  public uint ModelLookupTablesOffset { get; set; }

  // TODO: Handle everything else here

  [RAtPosition(nameof(ModelLookupTablesOffset))]
  public MeleeModelLookupTables ModelLookupTables { get; } = new();
}