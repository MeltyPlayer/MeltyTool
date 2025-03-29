using gm.schema.dataWin.chunk.sprt;
using gm.schema.dataWin.chunk.txtr;

using schema.binary;

namespace gm.schema.dataWin;

public class DataWin : IBinaryDeserializable {
  public Gen8 Gen8 { get; set; }
  public Sprt Sprt { get; set; }
  public Txtr Txtr { get; set; }

  public void Read(IBinaryReader br) {
    br.AssertString("FORM");
    var formSize = br.ReadInt32();

    while (!br.Eof) {
      var chunkType = br.ReadString(4);
      var chunkSize = br.ReadInt32();

      var baseOffset = br.Position;

      switch (chunkType) {
        case "GEN8": {
          this.Gen8 = br.ReadNew<Gen8>();
            break;
        }
        case "SPRT": {
          this.Sprt = br.ReadNew<Sprt>();
          break;
        }
        case "TXTR": {
          this.Txtr = br.ReadNew<Txtr>();
          break;
        }
      }

      br.Position = baseOffset + chunkSize;
    }
  }
}