using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using CommunityToolkit.Diagnostics;

using fin.math.matrix.three;

namespace fin.math.rotations;

public static class FinTrig {
  public const float DEG_2_RAD = MathF.PI / 180;
  public const float RAD_2_DEG = 1 / DEG_2_RAD;

  // - At this point, native C# approach is faster than FastMath.
  // - Math version is used instead of MathF because there are bit-level
  //   differences in MathF across machines that cause flakiness when comparing
  //   model files byte-by-byte. This was a fucking bitch to solve.

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Cos(float radians) => (float) Math.Cos(radians);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Sin(float radians) => (float) Math.Sin(radians);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Acos(float value) => (float) Math.Acos(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Asin(float value) => (float) Math.Asin(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Atan2(float y, float x) => (float) Math.Atan2(y, x);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void FromPitchYawRadians(float pitchRadians,
                                         float yawRadians,
                                         out float xNormal,
                                         out float yNormal,
                                         out float zNormal) {
    var horizontalNormal = Cos(pitchRadians);
    var verticalNormal = Sin(pitchRadians);

    xNormal = horizontalNormal * Cos(yawRadians);
    yNormal = horizontalNormal * Sin(yawRadians);
    zNormal = verticalNormal;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void FromPitchYawDegrees(float pitchDegrees,
                                         float yawDegrees,
                                         out float xNormal,
                                         out float yNormal,
                                         out float zNormal)
    => FromPitchYawRadians(pitchDegrees * DEG_2_RAD,
                           yawDegrees * DEG_2_RAD,
                           out xNormal,
                           out yNormal,
                           out zNormal);

  public static (float pitchDegrees, float yawDegrees)
      GetPitchYawDegreesFromTowards(in Vector3 from, in Vector3 to, int i) {
    var normal = Vector3.Normalize(to - from);

    var pitchDegrees = (float) Math.Atan2(normal.Z, normal.Xy().Length()) *
                       RAD_2_DEG;
    var yawDegrees = (float) Math.Atan2(normal.Y, normal.X) * RAD_2_DEG;

    return (pitchDegrees, yawDegrees);
  }

  public static Quaternion GetQuaternionTowards(
      in Vector3 normal,
      float rollRadians = 0) {
    var (pitchDegrees, yawDegrees)
        = FinTrig.GetPitchYawDegreesFromTowards(
            Vector3.Zero,
            normal,
            0);

    return QuaternionUtil.CreateZyxRadians(rollRadians,
                                           -pitchDegrees * DEG_2_RAD,
                                           yawDegrees * DEG_2_RAD);
  }

  public static Vector3 ConvertFromZUpToYUp(in Vector3 src)
    => new(src.X, src.Z, -src.Y);

  public static Quaternion ConvertFromZUpToYUp(in Quaternion src)
    => Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathF.PI / 2) * src;

  public static Quaternion ConvertFromYUpToZUp(in Quaternion src)
    => Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2) * src;

  public static (Quaternion quaternion0, Quaternion quaternion1)
      GetQuaternionsTowards(
          (float length, float rollRadians) tuple0,
          (float length, float rollRadians) tuple1,
          in Vector3 target)
    => GetQuaternionsTowards(tuple0, tuple1, target, out _, out _);

  public static (Quaternion quaternion0, Quaternion quaternion1)
      GetQuaternionsTowards(
          (float length, float rollRadians) tuple0,
          (float length, float rollRadians) tuple1,
          Vector3 target,
          out Vector3 forwardNormal,
          out Vector3 middlePoint) {
    forwardNormal = middlePoint = default;
    if (target.Length() == 0) {
      // TODO: What to do in this case?
      return (Quaternion.Identity, Quaternion.Identity);
    }

    var (length0, rollRadians0) = tuple0;
    var (length1, rollRadians1) = tuple1;
    Guard.IsGreaterThan(length0, 0);
    Guard.IsGreaterThan(length1, 0);

    var distance = MathF.Min(length0 + length1 - .001f, target.Length());
    forwardNormal = Vector3.Normalize(target);
    var feasibleTarget = forwardNormal * distance;

    Vector3 upNormal;
    if (forwardNormal.IsRoughly(Vector3.UnitZ)) {
      upNormal = -Vector3.UnitY;
    } else {
      upNormal = Vector3.UnitZ;
    }

    var rightNormal = Vector3.Normalize(Vector3.Cross(forwardNormal, upNormal));
    upNormal = Vector3.Normalize(Vector3.Cross(rightNormal, forwardNormal));

    var startingRadians
        = CalculateLawOfCosinesRadians_(length1, distance, length0);
    var middleRadians
        = CalculateLawOfCosinesRadians_(distance, length0, length1);
    var endRadians
        = CalculateLawOfCosinesRadians_(length0, length1, distance);

    var fromEndToMiddleNormal
        = Vector3.Transform(
            -forwardNormal,
            Quaternion.CreateFromAxisAngle(
                forwardNormal,
                rollRadians0) *
            Quaternion.CreateFromAxisAngle(
                -rightNormal,
                -endRadians));

    middlePoint = feasibleTarget + length1 * fromEndToMiddleNormal;
    var quaternion0 = FinTrig.GetQuaternionTowards(middlePoint, rollRadians0);
    var quaternion1 =
        FinTrig.GetQuaternionTowards(
            Vector3.Transform(target - middlePoint, Quaternion.Inverse(quaternion0)),
            rollRadians1);
    
    /*var quaternion1 =
        QuaternionUtil.CreateZyxRadians(rollRadians1,
                                        -(MathF.PI - middleRadians),
                                        0)*/;

    return (quaternion0, quaternion1);
  }

  private static float CalculateLawOfCosinesRadians_(
      float oppositeSideLength,
      float otherSideLength0,
      float otherSideLength1) {
    var numerator = otherSideLength0 * otherSideLength0 +
                    otherSideLength1 * otherSideLength1 -
                    oppositeSideLength * oppositeSideLength;
    var denominator = 2 * otherSideLength0 * otherSideLength1;

    var angle = MathF.Acos(numerator / denominator);

    return angle;
  }
}