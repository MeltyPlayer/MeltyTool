using schema.binary;

namespace visceral.schema.bnk;

public class Bnk : IBinaryDeserializable {
  public void Read(IBinaryReader br) {
      br.Position = 0x24;

      var animationHeaderCount = br.ReadUInt32();

      br.Position = 0x2C;
      var animationDataCount = br.ReadUInt32();
    }
}