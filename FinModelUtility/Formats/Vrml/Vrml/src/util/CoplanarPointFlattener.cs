using System.Numerics;

using fin.math;
using fin.math.matrix.four;

namespace vrml.util;

public static class CoplanarPointFlattener {
  public static IReadOnlyList<Vector2> FlattenCoplanarPoints(
      IReadOnlyList<Vector3> points3d) {
    if (points3d.Count < 3) {
      throw new NotSupportedException(
          "Need at least 3 points to get normal vector");
    }

    var flatteningMatrix = FindFlatteningMatrix_(points3d);
    return points3d
           .Select(point3d => {
             var point2d = Vector3.Transform(point3d, flatteningMatrix);
             return point2d.Xy();
           })
           .ToArray();
  }

  /// <summary>
  ///   Shamelessly stolen from: https://stackoverflow.com/a/49771112
  /// </summary>
  private static Matrix4x4 FindFlatteningMatrix_(
      IReadOnlyList<Vector3> points3d) {
    for (var i = 0; i < points3d.Count - 2; i++) {
      var b = points3d[i];
      var a = points3d[i + 1];
      var c = points3d[i + 2];

      var ab = a - b;
      var ac = a - c;

      var normal = Vector3.Cross(ab, ac);
      if (normal.IsRoughly0()) {
        continue;
      }

      var uU = Vector3.Normalize(ab);
      var uN = Vector3.Normalize(normal);
      var uV = Vector3.Cross(uU, uN);

      var u = a + uU;
      var v = a + uV;
      var n = a + uN;

      var matrixLhs = Matrix4x4.Transpose(new Matrix4x4(
          a.X,
          u.X,
          v.X,
          n.X,
          a.Y,
          u.Y,
          v.Y,
          n.Y,
          a.Z,
          u.Z,
          v.Z,
          n.Z,
          1,
          1,
          1,
          1));

      var matrixRhs
          = Matrix4x4.Transpose(
              new Matrix4x4(0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1));

      return matrixLhs.AssertInvert() * matrixRhs;
    }

    throw new InvalidDataException("Failed to find 2 non-colinear vectors");
  }
}