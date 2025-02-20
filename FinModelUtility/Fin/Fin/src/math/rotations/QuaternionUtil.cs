using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.xyz;
using fin.model;

using Quaternion = System.Numerics.Quaternion;

namespace fin.math.rotations;

public static class QuaternionUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Create(IRotation rotation)
    => CreateZyxRadians(rotation.XRadians,
                        rotation.YRadians,
                        rotation.ZRadians);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion CreateXyz(
      float xRadians,
      float yRadians,
      float zRadians) {
    return Quaternion.CreateFromAxisAngle(Vector3.UnitX, xRadians) *
           Quaternion.CreateFromAxisAngle(Vector3.UnitY, yRadians) *
           Quaternion.CreateFromAxisAngle(Vector3.UnitZ, zRadians);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion CreateZyxRadians(
      float xRadians,
      float yRadians,
      float zRadians) {
    var cr = FinTrig.Cos(xRadians * 0.5f);
    var sr = FinTrig.Sin(xRadians * 0.5f);
    var cp = FinTrig.Cos(yRadians * 0.5f);
    var sp = FinTrig.Sin(yRadians * 0.5f);
    var cy = FinTrig.Cos(zRadians * 0.5f);
    var sy = FinTrig.Sin(zRadians * 0.5f);

    return new Quaternion(
        sr * cp * cy - cr * sp * sy,
        cr * sp * cy + sr * cp * sy,
        cr * cp * sy - sr * sp * cy,
        cr * cp * cy + sr * sp * sy);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion CreateZyxRadians(in this Vector3 xyzRadians)
    => CreateZyxRadians(xyzRadians.X, xyzRadians.Y, xyzRadians.Z);

  // TODO: Slow! Figure out how to populate animations with raw quaternions instead
  public static Vector3 ToEulerRadians(in this Quaternion q) {
    if (q.IsIdentity) {
      return Vector3.Zero;
    }

    Vector3 angles;

    var qy2 = q.Y * q.Y;

    // roll / x
    var sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
    var cosr_cosp = 1 - 2 * (q.X * q.X + qy2);
    angles.X = FinTrig.Atan2(sinr_cosp, cosr_cosp);

    // pitch / y
    var sinp = (float) (2 * (q.W * q.Y - q.Z * q.X));
    if (Math.Abs(sinp) >= 1) {
      angles.Y = MathF.CopySign(MathF.PI / 2, sinp);
    } else {
      angles.Y = FinTrig.Asin(sinp);
    }

    // yaw / z
    var siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
    var cosy_cosp = 1 - 2 * (qy2 + q.Z * q.Z);
    angles.Z = FinTrig.Atan2(siny_cosp, cosy_cosp);

    return angles;
  }
}