using System.Runtime.CompilerServices;

using fin.math.floats;

namespace fin.animation.keyframes;

public static class FrameComparisonUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool AreSameFrame(float lhs, float rhs)
    => lhs.IsRoughly(rhs);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int CompareFrames(float lhs, float rhs)
    => AreSameFrame(lhs, rhs) ? 0 : lhs.CompareTo(rhs);
}