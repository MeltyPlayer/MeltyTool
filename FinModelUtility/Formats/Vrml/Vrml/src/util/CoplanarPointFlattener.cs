using System.Numerics;

using fin.math;
using fin.math.matrix.four;

using Newtonsoft.Json.Linq;

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
    var options = new LinkedList<(Vector3 a, Vector3 ab, Vector3 normal)>();
    for (var i0 = 0; i0 < points3d.Count; i0++) {
      var a = points3d[i0];

      for (var i1 = i0 + 1; i1 < points3d.Count; i1++) {
        var b = points3d[i1];
        var ab = Vector3.Normalize(a - b);
        
        for (var i2 = i1 + 1; i2 < points3d.Count; i2++) {
          var c = points3d[i2];

          var ac = Vector3.Normalize(a - c);
          var normal = Vector3.Cross(ab, ac);

          options.AddLast((a, ab, normal));
        }
      }
    }

    var bestOption = options.MaxBy(o => o.normal.Length());
    if (bestOption.normal.IsRoughly0()) {
      throw new InvalidDataException("Failed to find 2 non-colinear vectors");
    }

    return GetFlatteningMatrix_(bestOption.a, bestOption.ab, bestOption.normal);
  }

  private static Matrix4x4 GetFlatteningMatrix_(Vector3 a,
                                                Vector3 ab,
                                                Vector3 normal) {
    var uU = ab;
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
}