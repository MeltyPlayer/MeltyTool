using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.floats;
using fin.math.rotations;
using fin.util.hash;


namespace fin.math.matrix.three;

public static class SystemMatrix3x2Util {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe bool IsRoughly(this Matrix3x2 lhs, Matrix3x2 rhs) {
    float* lhsPtr = &lhs.M11;
    float* rhsPtr = &rhs.M11;

    for (var i = 0; i < 3 * 2; ++i) {
      if (!lhsPtr[i].IsRoughly(rhsPtr[i])) {
        return false;
      }
    }

    return true;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe int GetRoughHashCode(this Matrix3x2 mat) {
    var error = FloatsExtensions.ROUGHLY_EQUAL_ERROR;

    var hash = new FluentHash();
    float* ptr = &mat.M11;
    for (var i = 0; i < 3 * 2; ++i) {
      var value = MathF.Round(ptr[i] / error) * error;
      hash = hash.With(value.GetHashCode());
    }

    return hash;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 FromTranslation(Vector2 translation)
    => FromTranslation(translation.X, translation.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 FromTranslation(float x, float y)
    => Matrix3x2.CreateTranslation(x, y);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 FromRotation(float radians)
    => Matrix3x2.CreateRotation(radians);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 FromScale(float scale)
    => SystemMatrix3x2Util.FromScale(scale, scale);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 FromScale(Vector2 scale)
    => FromScale(scale.X, scale.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 FromScale(float scaleX, float scaleY)
    => Matrix3x2.CreateScale(scaleX, scaleY);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 FromSkewXRadians(float skewXRadians)
    => Matrix3x2.CreateSkew(skewXRadians, 0);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 FromCtrss(
      Vector2? center,
      Vector2? translation,
      float? rotationRadians,
      Vector2? scale,
      float? skewXRadians) {
    var dst = Matrix3x2.Identity;

    if (center != null) {
      dst = Matrix3x2.Multiply(FromTranslation(center.Value), dst);
    }

    if (translation != null) {
      dst = Matrix3x2.Multiply(FromTranslation(translation.Value), dst);
    }

    if (rotationRadians != null) {
      dst = Matrix3x2.Multiply(FromRotation(rotationRadians.Value), dst);
    }

    if (scale != null) {
      dst = Matrix3x2.Multiply(FromScale(scale.Value), dst);
    }

    if (skewXRadians != null) {
      dst = Matrix3x2.Multiply(FromSkewXRadians(skewXRadians.Value), dst);
    }

    if (center != null) {
      dst = Matrix3x2.Multiply(FromTranslation(-center.Value), dst);
    }

    return dst;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 FromTrss(
      Vector2? translation,
      float? rotationRadians,
      Vector2? scale,
      float? skewXRadians) {
    var dst = Matrix3x2.Identity;

    if (translation != null) {
      dst = Matrix3x2.Multiply(FromTranslation(translation.Value), dst);
    }

    if (rotationRadians != null) {
      dst = Matrix3x2.Multiply(FromRotation(rotationRadians.Value), dst);
    }

    if (scale != null) {
      dst = Matrix3x2.Multiply(FromScale(scale.Value), dst);
    }

    if (skewXRadians != null) {
      dst = Matrix3x2.Multiply(FromSkewXRadians(skewXRadians.Value), dst);
    }

    return dst;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 GetTranslation(this Matrix3x2 impl)
    => impl.Translation;

  // Stolen from https://stackoverflow.com/a/32125700
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetRotation(this Matrix3x2 impl)
    => FinTrig.Atan2(impl.M12, impl.M11);

  // Stolen from https://stackoverflow.com/a/32125700
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 GetScale(this Matrix3x2 impl) {
    var scaleX = MathF.Sqrt(impl.M11 * impl.M11 +
                            impl.M12 * impl.M12);
    var scaleY =
        (impl.M11 * impl.M22 -
         impl.M21 * impl.M12) /
        scaleX;
    return new Vector2(scaleX, scaleY);
  }

  // Stolen from https://stackoverflow.com/a/32125700
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetSkewXRadians(this Matrix3x2 impl)
    => MathF.Atan2(impl.M11 * impl.M21 +
                   impl.M12 * impl.M22,
                   impl.M11 * impl.M11 +
                   impl.M12 * impl.M12);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Decompose(
      this Matrix3x2 impl,
      out Vector2 translation,
      out float rotation,
      out Vector2 scale,
      out float skewXRadians) {
    // Stolen from https://stackoverflow.com/a/32125700
    translation = impl.GetTranslation();
    rotation = impl.GetRotation();
    scale = impl.GetScale();
    skewXRadians = impl.GetSkewXRadians();
  }
}