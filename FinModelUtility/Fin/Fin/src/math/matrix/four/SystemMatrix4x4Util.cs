using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.floats;
using fin.math.rotations;
using fin.model;
using fin.util.asserts;
using fin.util.hash;

using SixLabors.ImageSharp.ColorSpaces;


namespace fin.math.matrix.four;

public static class SystemMatrix4x4Util {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe bool IsRoughly(this in Matrix4x4 lhs, in Matrix4x4 rhs) {
    fixed (float* lhsPtr = &lhs.M11) {
      fixed (float* rhsPtr = &rhs.M11) {
        for (var i = 0; i < 4 * 4; ++i) {
          if (!lhsPtr[i].IsRoughly(rhsPtr[i])) {
            return false;
          }
        }
      }
    }

    return true;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe int GetRoughHashCode(this in Matrix4x4 mat) {
    var error = FloatsExtensions.ROUGHLY_EQUAL_ERROR;

    var hash = new FluentHash();
    fixed (float* ptr = &mat.M11) {
      for (var i = 0; i < 4 * 4; ++i) {
        var value = MathF.Round(ptr[i] / error) * error;
        hash = hash.With(value.GetHashCode());
      }
    }

    return hash;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromTranslation(in Vector3 translation)
    => Matrix4x4.CreateTranslation(translation);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromTranslation(float x, float y, float z)
    => Matrix4x4.CreateTranslation(x, y, z);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromRotation(IRotation rotation)
    => FromRotation(QuaternionUtil.Create(rotation));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromRotation(in Quaternion rotation)
    => Matrix4x4.CreateFromQuaternion(rotation);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromScale(in Vector3 scale)
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 FromCtrs(
      Vector3? center,
      Vector3? translation,
      Quaternion? rotation,
      Vector3? scale) {
    var dst = Matrix4x4.Identity;

    if (center != null) {
      dst = Matrix4x4.Multiply(FromTranslation(center.Value), dst);
    }

    if (translation != null) {
      dst = Matrix4x4.Multiply(FromTranslation(translation.Value), dst);
    }

    if (rotation != null) {
      dst = Matrix4x4.Multiply(FromRotation(rotation.Value), dst);
    }

    if (scale != null) {
      dst = Matrix4x4.Multiply(FromScale(scale.Value), dst);
    }

    if (center != null) {
      dst = Matrix4x4.Multiply(FromTranslation(-center.Value), dst);
    }

    return dst;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 AssertInvert(this in Matrix4x4 matrix) {
    Asserts.True(Matrix4x4.Invert(matrix, out var inverted) ||
                 !FinMatrix4x4.STRICT_INVERTING,
                 "Failed to invert matrix!");
    return inverted;
  }
}