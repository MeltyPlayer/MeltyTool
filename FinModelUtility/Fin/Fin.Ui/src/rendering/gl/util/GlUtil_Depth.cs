using fin.model;
using fin.util.enums;

using OpenTK.Graphics.ES30;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public (DepthMode, DepthCompareType) DepthModeAndCompareType { get; set; }
    = (DepthMode.READ | DepthMode.WRITE, DepthCompareType.LEqual);
}

public static partial class GlUtil {
  public static bool DisableChangingDepth { get; set; }

  public static void ResetDepth()
    => SetDepth((DepthMode.READ | DepthMode.WRITE), DepthCompareType.LEqual);

  public static bool SetDepth(DepthMode depthMode)
    => SetDepth(depthMode, depthMode == DepthMode.NONE ? DepthCompareType.Always : DepthCompareType.LEqual);

  public static bool SetDepth(
      DepthMode depthMode,
      DepthCompareType depthCompareType) {
    if (DisableChangingDepth) {
      return false;
    }

    if (currentState_.DepthModeAndCompareType ==
        (depthMode, depthCompareType)) {
      return false;
    }

    currentState_.DepthModeAndCompareType
        = (depthMode, depthCompareType);

    if (depthMode.CheckFlag(DepthMode.READ) &&
        depthCompareType != DepthCompareType.Always) {
      GL.DepthFunc(ConvertFinDepthCompareTypeToGl_(depthCompareType));
      GL.Enable(EnableCap.DepthTest);
    } else {
      GL.Disable(EnableCap.DepthTest);
    }

    GL.DepthMask(depthMode.CheckFlag(DepthMode.WRITE));

    return true;
  }

  private static DepthFunction ConvertFinDepthCompareTypeToGl_(
      DepthCompareType finDepthCompareType)
    => finDepthCompareType switch {
        DepthCompareType.LEqual  => DepthFunction.Lequal,
        DepthCompareType.Less    => DepthFunction.Less,
        DepthCompareType.Equal   => DepthFunction.Equal,
        DepthCompareType.Greater => DepthFunction.Greater,
        DepthCompareType.NEqual  => DepthFunction.Notequal,
        DepthCompareType.GEqual  => DepthFunction.Gequal,
        DepthCompareType.Always  => DepthFunction.Always,
        DepthCompareType.Never   => DepthFunction.Never,
        _ => throw new ArgumentOutOfRangeException(
            nameof(finDepthCompareType),
            finDepthCompareType,
            null)
    };
}