using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.math.matrix.three;

public static class FinMatrix3x2Util {
  public static IReadOnlyFinMatrix3x2 IDENTITY { get; } =
    FinMatrix3x2Util.FromIdentity();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromIdentity()
    => new FinMatrix3x2().SetIdentity();


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromTranslation(Vector2 translation)
    => new FinMatrix3x2(
        SystemMatrix3x2Util.FromTranslation(translation.X, translation.Y));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromTranslation(float x, float y)
    => new FinMatrix3x2(SystemMatrix3x2Util.FromTranslation(x, y));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromRotation(float radians)
    => new FinMatrix3x2(SystemMatrix3x2Util.FromRotation(radians));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromScale(float scale)
    => FromScale(scale, scale);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromScale(Vector2 scale)
    => new FinMatrix3x2(SystemMatrix3x2Util.FromScale(scale.X, scale.Y));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromScale(float scaleX, float scaleY)
    => new FinMatrix3x2(SystemMatrix3x2Util.FromScale(scaleX, scaleY));


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromSkewXRadians(float skewXRadians)
    => new FinMatrix3x2(SystemMatrix3x2Util.FromSkewXRadians(skewXRadians));


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromTrss(Vector2? translation,
                                       float? rotationRadians,
                                       Vector2? scale,
                                       float? skewXRadians)
    => FinMatrix3x2Util.FromTrss(translation, rotationRadians, scale, skewXRadians, new FinMatrix3x2());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3x2 FromTrss(Vector2? translation,
                                       float? rotationRadians,
                                       Vector2? scale,
                                       float? skewXRadians,
                                       IFinMatrix3x2 dst) {
    dst.CopyFrom(
        SystemMatrix3x2Util.FromTrss(translation,
                                     rotationRadians,
                                     scale,
                                     skewXRadians));
    return dst;
  }
}