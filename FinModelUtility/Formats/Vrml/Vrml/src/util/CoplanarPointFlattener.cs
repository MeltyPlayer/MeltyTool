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
    for (var i = 0; i < points3d.Count; i++) {
      if (TryToGetFlatteningMatrixFromVectors_(
              points3d[(i + 1) % points3d.Count],
              points3d[i],
              points3d[(i + 2) % points3d.Count],
              out var flatteningMatrix)) {
        return flatteningMatrix;
      }
    }

    for (var i0 = 0; i0 < points3d.Count; i0++) {
      for (var i1 = i0 + 1; i1 < points3d.Count; i1++) {
        for (var i2 = i1 + 1; i2 < points3d.Count; i2++) {
          if (TryToGetFlatteningMatrixFromVectors_(
                  points3d[i0],
                  points3d[i1],
                  points3d[i2],
                  out var flatteningMatrix)) {
            return flatteningMatrix;
          }
        }
      }
    }

    throw new InvalidDataException("Failed to find 2 non-colinear vectors");
  }

  private static bool TryToGetFlatteningMatrixFromVectors_(
      Vector3 a,
      Vector3 b,
      Vector3 c,
      out Matrix4x4 flatteningMatrix) {
    var ab = a - b;
    var ac = a - c;

    var normal = Vector3.Cross(ab, ac);
    if (normal.IsRoughly0()) {
      flatteningMatrix = default;
      return false;
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

    flatteningMatrix = matrixLhs.AssertInvert() * matrixRhs;
    return true;
  }
}