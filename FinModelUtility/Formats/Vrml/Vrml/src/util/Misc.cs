using System.Numerics;

using fin.math.floats;

namespace vrml.util;

class Misc {
  public static int GetOrientation(in Vector3 v0,
                                   in Vector3 v1,
                                   in Vector3 v2,
                                   in Vector3 normal) {
    var res = Vector3.Cross(v0 - v1, v2 - v1);
    if (res.Length().IsRoughly0()) {
      return 0;
    }

    return Vector3.Dot(res, normal) < 0 ? 1 : -1;
  }

  public static bool PointInOrOnTriangle(in Vector3 prevPoint,
                                         in Vector3 curPoint,
                                         in Vector3 nextPoint,
                                         in Vector3 nonConvexPoint,
                                         in Vector3 normal) {
    var res0 = Misc.GetOrientation(prevPoint, nonConvexPoint, curPoint, normal);
    var res1 = Misc.GetOrientation(curPoint, nonConvexPoint, nextPoint, normal);
    var res2
        = Misc.GetOrientation(nextPoint, nonConvexPoint, prevPoint, normal);
    return res0 != 1 && res1 != 1 && res2 != 1;
  }
}