using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.math.matrix.four;

public static class ProjectionUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ProjectPosition(in Matrix4x4 matrix, ref Vector3 xyz)
    => xyz = Vector3.Transform(xyz, matrix);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ProjectNormal(in Matrix4x4 matrix, ref Vector3 xyz)
    => xyz = Vector3.TransformNormal(xyz, matrix);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ProjectTangent(in Matrix4x4 matrix, ref Vector4 xyzw)
      // TODO: Might be wrong
    => xyzw = Vector4.Transform(xyzw, matrix);
}