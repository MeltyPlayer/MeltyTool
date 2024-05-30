using schema.binary;

namespace ttyd.schema.model.blocks {
  [BinarySchema]
  public partial class TextureMap : IBinaryDeserializable {
    public int TextureIndex { get; set; }
    public uint Unk { get; set; }
  }
}