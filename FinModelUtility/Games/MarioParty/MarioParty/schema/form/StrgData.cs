using LinkDotNet.StringBuilder;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L523
/// </summary>
[BinarySchema]
[LocalPositions]
public sealed partial class StrgData : IFormChunkData {
  private ushort stringCount_;

  [RSequenceLengthSource(nameof(stringCount_))]
  private byte[] stringLengths_;

  [Skip]
  public string[] Strings { get; private set; }

  [ReadLogic]
  private void ReadStrings_(IBinaryReader br) {
    this.Strings = new string[this.stringCount_];
    for (var i = 0; i < this.Strings.Length; i++) {
      using var sb = new ValueStringBuilder();
      for (var j = 0; j < this.stringLengths_[i]; ++j) {
        sb.Append(br.ReadChar());
      }
      this.Strings[i] = sb.ToString();
    }
  }
}