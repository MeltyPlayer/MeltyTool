using schema.binary;

namespace ttyd.schema.model.blocks;

[Flags]
public enum WrapFlags : int {
  REPEAT_S = 0x1,
  REPEAT_T = 0x2,
  MIRROR_S = 0x4,
  MIRROR_T = 0x8,
}

[BinarySchema]
public partial record Sampler : IBinaryDeserializable {
  public int TextureIndex { get; set; }
  public WrapFlags WrapFlags { get; set; }
}