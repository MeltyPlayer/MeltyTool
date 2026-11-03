using fin.schema.data;
using fin.util.types;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts
/// </summary>
[BinarySchema]
public sealed partial class Form : IBinaryDeserializable {
  private readonly string magic_ = "FORM";

  [RSequenceUntilEndOfStream]
  public FormChunk[] Chunks { get; set; }
}

[BinarySchema]
[AlignEnd(2)]
public sealed partial class FormChunk : IBinaryDeserializable {
  public SwitchMagicStringUInt32SizedSection<IFormChunkData> Impl { get; }
    = new(4,
          magic => magic switch {
              "ATR1" => new Atr1Data(),
              "COL1" => new Col1Data(),
              "HBIN" => new HbinmodeData(),
              "MAT1" => new Mat1Data(),
              "STRG" => new StrgData(),
              _      => throw new NotImplementedException(),
          });
}

[UnionCandidate]
public interface IFormChunkData : IBinaryConvertible;