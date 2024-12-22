using schema.binary;

namespace sm64.schema;

[Flags]
public enum AnimationFlags : short {
  NO_LOOP = 1 << 0,
  BACKWARD = 1 << 1,
  _2 = 1 << 2,
  HOR_TRANS = 1 << 3,
  VERT_TRANS = 1 << 4,
  _5 = 1 << 5,
  _6 = 1 << 6,
  _7 = 1 << 7,
}

[BinarySchema]
public partial class Animation : IBinaryConvertible {
  public AnimationFlags Flags { get; set; }
  public short YTransDivisor { get; set; }
  public short StartFrame { get; set; }
  public short LoopStart { get; set; }
  public short LoopEnd { get; set; }
  public short UnusedBoneCount { get; set; }
}