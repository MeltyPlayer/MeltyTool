using fin.schema;

using schema.binary;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L208
/// </summary>
[BinarySchema]
public sealed partial class HbinmodeData : IFormChunkData {
  private readonly string magic_ = "MODE";

  [Unknown]
  private readonly byte[] unk_ = new byte [0x14];

  public byte E { get; set; }
  public byte F { get; set; }
  public byte G { get; set; }
}