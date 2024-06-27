using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.rotations;
using fin.model;

namespace fin.math.matrix.four {
  public static class FinMatrix4x4Util {
    public static IReadOnlyFinMatrix4x4 IDENTITY { get; } =
      FinMatrix4x4Util.FromIdentity();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromIdentity()
      => new FinMatrix4x4().SetIdentity();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromTranslation(Vector3 translation)
      => FinMatrix4x4Util.FromTranslation(
          translation.X,
          translation.Y,
          translation.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromTranslation(float x, float y, float z)
      => new FinMatrix4x4(SystemMatrix4x4Util.FromTranslation(x, y, z));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromRotation(IRotation rotation)
      => FinMatrix4x4Util.FromRotation(QuaternionUtil.Create(rotation));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromRotation(Quaternion rotation)
      => new FinMatrix4x4(SystemMatrix4x4Util.FromRotation(rotation));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromScale(Vector3 scale)
      => FromScale(scale.X, scale.Y, scale.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromScale(float scale)
      => FromScale(scale, scale, scale);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromScale(float scaleX,
                                          float scaleY,
                                          float scaleZ)
      => new FinMatrix4x4(
          SystemMatrix4x4Util.FromScale(scaleX, scaleY, scaleZ));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromTrs(
        Vector3? translation,
        IRotation? rotation,
        Vector3? scale)
      => FinMatrix4x4Util.FromTrs(
          translation,
          rotation != null ? QuaternionUtil.Create(rotation) : null,
          scale);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromTrs(
        Vector3? translation,
        Quaternion? rotation,
        Vector3? scale)
      => FromTrs(translation, rotation, scale, new FinMatrix4x4());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFinMatrix4x4 FromTrs(
        Vector3? translation,
        Quaternion? rotation,
        Vector3? scale,
        IFinMatrix4x4 dst) {
      dst.CopyFrom(SystemMatrix4x4Util.FromTrs(translation, rotation, scale));
      return dst;
    }

    public static Vector3 TransformPosition(this IReadOnlyFinMatrix4x4 matrix,
                                            in Vector3 position)
      => Vector3.Transform(position, matrix.Impl);

    public static Vector2 TransformPosition(this IReadOnlyFinMatrix4x4 matrix,
                                            in Vector2 position)
      => Vector2.Transform(position, matrix.Impl);
  }
}