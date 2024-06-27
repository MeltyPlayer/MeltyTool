using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.model.blocks;

[BinarySchema]
public partial class Texture : IBinaryDeserializable {
  public uint Unk1 { get; set; }
  public int TplTextureIndex { get; set; }
  public uint Unk2 { get; set; }

  [StringLengthSource(52)]
  public string Name { get; set; }
}