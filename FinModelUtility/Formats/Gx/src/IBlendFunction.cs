namespace gx;

public enum GxBlendMode : byte {
  NONE = 0,
  BLEND = 1,
  LOGIC = 2,
  SUBTRACT = 3,
}

public enum GxBlendFactor : byte {
  ZERO = 0,
  ONE = 1,
  SRC_COLOR = 2,
  ONE_MINUS_SRC_COLOR = 3,
  SRC_ALPHA = 4,
  ONE_MINUS_SRC_ALPHA = 5,
  DST_ALPHA = 6,
  ONE_MINUS_DST_ALPHA = 7,
}

public enum GxLogicOp : byte {
  CLEAR = 0,
  AND = 1,
  AND_REVERSE = 2,
  COPY = 3,
  AND_INVERTED = 4,
  NOOP = 5,
  XOR = 6,
  OR = 7,
  NOR = 8,
  EQUIV = 9,
  INVERT = 10,
  OR_REVERSE = 11,
  COPY_INVERTED = 12,
  OR_INVERTED = 13,
  NAND = 14,
  SET = 15,
}

public interface IBlendFunction {
  GxBlendMode BlendMode { get; }
  GxBlendFactor SrcFactor { get; }
  GxBlendFactor DstFactor { get; }
  GxLogicOp LogicOp { get; }
}

public class BlendFunctionImpl : IBlendFunction {
  public GxBlendMode BlendMode { get; set; }
  public GxBlendFactor SrcFactor { get; set; }
  public GxBlendFactor DstFactor { get; set; }
  public GxLogicOp LogicOp { get; set; }
}