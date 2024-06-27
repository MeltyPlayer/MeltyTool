using System;
using System.Runtime.CompilerServices;

namespace fin.math.rotations;

public static class FinTrig {
  public const float DEG_2_RAD = MathF.PI / 180;
  public const float RAD_2_DEG = 1 / FinTrig.DEG_2_RAD;


  // At this point, native C# approach is faster than FastMath.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Cos(float radians) => MathF.Cos(radians);

  // At this point, native C# approach is faster than FastMath.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Sin(float radians) => MathF.Sin(radians);

  // At this point, native C# approach is faster than FastMath.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Acos(float value) => MathF.Acos(value);

  // At this point, native C# approach is faster than FastMath.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Asin(float value) => MathF.Asin(value);

  // At this point, native C# approach is faster than FastMath.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Atan2(float y, float x) => MathF.Atan2(y, x);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void FromPitchYawRadians(float pitchRadians,
                                         float yawRadians,
                                         out float xNormal,
                                         out float yNormal,
                                         out float zNormal) {
    var horizontalNormal = FinTrig.Cos(pitchRadians);
    var verticalNormal = FinTrig.Sin(pitchRadians);

    xNormal = horizontalNormal * FinTrig.Cos(yawRadians);
    yNormal = horizontalNormal * FinTrig.Sin(yawRadians);
    zNormal = verticalNormal;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void FromPitchYawDegrees(float pitchDegrees,
                                         float yawDegrees,
                                         out float xNormal,
                                         out float yNormal,
                                         out float zNormal)
    => FinTrig.FromPitchYawRadians(pitchDegrees * FinTrig.DEG_2_RAD,
                                   yawDegrees * FinTrig.DEG_2_RAD,
                                   out xNormal,
                                   out yNormal,
                                   out zNormal);
}