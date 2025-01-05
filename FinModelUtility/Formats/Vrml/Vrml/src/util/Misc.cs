using System.Numerics;

using fin.math.floats;
using fin.math.matrix.three;

namespace vrml.util;

class Misc {
  public static int GetOrientation(Vector3 v0,
                                   Vector3 v1,
                                   Vector3 v2,
                                   Vector3 normal) {
    var res = Vector3.Cross(v0 - v1, v2 - v1);
    if (res.Length().IsRoughly0()) {
      return 0;
    }

    if (Math.Sign(res.X) != Math.Sign(normal.X) ||
        Math.Sign(res.Y) != Math.Sign(normal.Y) ||
        Math.Sign(res.Z) != Math.Sign(normal.Z))
      return 1;
    return -1;
  }

  public static bool PointInOrOnTriangle(Vector3 prevPoint,
                                         Vector3 curPoint,
                                         Vector3 nextPoint,
                                         Vector3 nonConvexPoint,
                                         Vector3 normal) {
    var res0 = Misc.GetOrientation(prevPoint, nonConvexPoint, curPoint, normal);
    var res1 = Misc.GetOrientation(curPoint, nonConvexPoint, nextPoint, normal);
    var res2
        = Misc.GetOrientation(nextPoint, nonConvexPoint, prevPoint, normal);
    return res0 != 1 && res1 != 1 && res2 != 1;
  }
}