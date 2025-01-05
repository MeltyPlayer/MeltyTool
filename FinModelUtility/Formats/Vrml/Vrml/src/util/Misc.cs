namespace vrml.util;

class Misc {
  public static int GetOrientation(Vector3m v0,
                                   Vector3m v1,
                                   Vector3m v2,
                                   Vector3m normal) {
    var res = (v0 - v1).Cross(v2 - v1);
    if (res.LengthSquared().ToDouble() == 0)
      return 0;
    if (res.X.Sign != normal.X.Sign ||
        res.Y.Sign != normal.Y.Sign ||
        res.Z.Sign != normal.Z.Sign)
      return 1;
    return -1;
  }

  public static bool PointInOrOnTriangle(Vector3m prevPoint,
                                         Vector3m curPoint,
                                         Vector3m nextPoint,
                                         Vector3m nonConvexPoint,
                                         Vector3m normal) {
    var res0 = Misc.GetOrientation(prevPoint, nonConvexPoint, curPoint, normal);
    var res1 = Misc.GetOrientation(curPoint, nonConvexPoint, nextPoint, normal);
    var res2
        = Misc.GetOrientation(nextPoint, nonConvexPoint, prevPoint, normal);
    return res0 != 1 && res1 != 1 && res2 != 1;
  }
}