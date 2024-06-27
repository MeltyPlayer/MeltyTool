using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.floats;
using fin.math.rotations;
using fin.model;
using fin.util.hash;


namespace fin.math.matrix.four;

public static class SystemMatrix4x4Util {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe bool IsRoughly(this Matrix4x4 lhs, Matrix4x4 rhs) {
    float* lhsPtr = &lhs.M11;
    float* rhsPtr = &rhs.M11;

    for (var i = 0; i < 4 * 4; ++i) {
      if (!lhsPtr[i].IsRoughly(rhsPtr[i])) {
        return false;
      }
    }

    return true;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe int GetRoughHashCode(this Matrix4x4 mat) {
    var error = FloatsExtensions.ROUGHLY_EQUAL_ERROR;

    var hash = new FluentHash();
    float* ptr = &mat.M11;
    for (var i = 0; i < 4 * 4; ++i) {
      var value = MathF.Round(ptr[i] / error) * error;
      hash = hash.With(value.GetHashCode());
    }

    return hash;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromTranslation(Vector3 translation)
    => Matrix4x4.CreateTranslation(translation);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromTranslation(float x, float y, float z)
    => Matrix4x4.CreateTranslation(x, y, z);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromRotation(IRotation rotation)
    => SystemMatrix4x4Util.FromRotation(QuaternionUtil.Create(rotation));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromRotation(Quaternion rotation)
    => Matrix4x4.CreateFromQuaternion(rotation);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromScale(Vector3 scale)
    => Matrix4x4.CreateScale(scale);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromScale(float scale)
    => Matrix4x4.CreateScale(scale, scale, scale);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromScale(float scaleX, float scaleY, float scaleZ)
    => Matrix4x4.CreateScale(scaleX, scaleY, scaleZ);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromTrs(
      Vector3? translation,
      IRotation? rotation,
      Vector3? scale)
    => FromTrs(
        translation,
        rotation != null ? QuaternionUtil.Create(rotation) : null,
        scale);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromTrs(
      Vector3? translation,
      Quaternion? rotation,
      Vector3? scale) {
    var dst = rotation != null
        ? FromRotation(rotation.Value)
        : Matrix4x4.Identity;

    if (translation != null) {
      dst.Translation = translation.Value;
    }

    if (scale != null) {
      dst = Matrix4x4.Multiply(Matrix4x4.CreateScale(scale.Value), dst);
    }

    return dst;
  }
}