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

  private uint fileSize_;

  [RSequenceUntilEndOfStream]
  public FormChunk[] Chunks { get; set; }
}

[BinarySchema]
[AlignEnd(2)]
public sealed partial class FormChunk : IBinaryDeserializable {
  [ReadLogic]
  private void SetImpl_(IBinaryReader br) {
    var magic = br.ReadString(4);

    if (magic == "HBIN") {
      br.AssertString("MODE");
    }

    this.Impl = new(magic switch {
        "ATR1" => new Atr1Data(),
        "BMP1" => new Bmp1Data(),
        "COL1" => new Col1Data(),
        "FAC1" => new Fac1Data(),
        "HBIN" => new HbinmodeData(),
        "MAT1" => new Mat1Data(),
        "MTN1" => new NoopData(),
        "OBJ1" => new Obj1Data(),
        "PAL1" => new Pal1Data(),
        "SKL1" => new Skl1Data(),
        "STRG" => new StrgData(),
        "VTX1" => new Vtx1Data(),
        _      => throw new NotImplementedException(),
    });
  }

  public PassThruUInt32SizedSection<IFormChunkData> Impl { get; private set; }
}

[UnionCandidate]
public interface IFormChunkData : IBinaryConvertible;