using fin.model;

namespace gx;

public enum GxCompareType : byte {
  Never = 0,
  Less = 1,
  Equal = 2,
  LEqual = 3,
  Greater = 4,
  NEqual = 5,
  GEqual = 6,
  Always = 7
}

public static class GxCompareTypeExtensions {
  public static DepthCompareType ToFinDepthCompareType(
      this GxCompareType gxDepthCompareType)
    => gxDepthCompareType switch {
        GxCompareType.Never   => DepthCompareType.Never,
        GxCompareType.Less    => DepthCompareType.Less,
        GxCompareType.Equal   => DepthCompareType.Equal,
        GxCompareType.LEqual  => DepthCompareType.LEqual,
        GxCompareType.Greater => DepthCompareType.Greater,
        GxCompareType.NEqual  => DepthCompareType.NEqual,
        GxCompareType.GEqual  => DepthCompareType.GEqual,
        GxCompareType.Always  => DepthCompareType.Always,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gxDepthCompareType),
            gxDepthCompareType,
            null)
    };

  public static AlphaCompareType ToFinAlphaCompareType(
      this GxCompareType gxAlphaCompareType)
    => gxAlphaCompareType switch {
        GxCompareType.Never   => AlphaCompareType.Never,
        GxCompareType.Less    => AlphaCompareType.Less,
        GxCompareType.Equal   => AlphaCompareType.Equal,
        GxCompareType.LEqual  => AlphaCompareType.LEqual,
        GxCompareType.Greater => AlphaCompareType.Greater,
        GxCompareType.NEqual  => AlphaCompareType.NEqual,
        GxCompareType.GEqual  => AlphaCompareType.GEqual,
        GxCompareType.Always  => AlphaCompareType.Always,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gxAlphaCompareType),
            gxAlphaCompareType,
            null)
    };
}


public enum GxAlphaOp : byte {
  And = 0,
  Or = 1,
  XOR = 2,
  XNOR = 3
}

public static class GxAlphaOpExtensions {
  public static AlphaOp ToFinAlphaOp(this GxAlphaOp gxAlphaOp)
    => gxAlphaOp switch {
        GxAlphaOp.And  => AlphaOp.And,
        GxAlphaOp.Or   => AlphaOp.Or,
        GxAlphaOp.XOR  => AlphaOp.XOR,
        GxAlphaOp.XNOR => AlphaOp.XNOR,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gxAlphaOp),
            gxAlphaOp,
            null)
    };
}


public interface IAlphaCompare {
  GxCompareType Func0 { get; }
  float Reference0 { get; }

  GxCompareType Func1 { get; }
  float Reference1 { get; }

  GxAlphaOp MergeFunc { get; }
}

public class AlphaCompareImpl : IAlphaCompare {
  public GxCompareType Func0 { get; set; }
  public float Reference0 { get; set; }
  public GxCompareType Func1 { get; set; }
  public float Reference1 { get; set; }
  public GxAlphaOp MergeFunc { get; set; }
}