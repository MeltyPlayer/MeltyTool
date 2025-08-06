using System.Numerics;

using fin.math;
using fin.math.fixedPoint;


namespace f3dzex2.image;

public static class TmemUtil {
  public static float ParseCoordAxis(ushort fixedPointCoordAxis)
    => FixedPointFloatUtil.Convert16(
        (ushort) (fixedPointCoordAxis & 0xFFF),
        false,
        10,
        2);

  public static Vector2 ParseCoordAxes(uint fixedPointCoordAxes)
    => new(
        ParseCoordAxis((ushort) fixedPointCoordAxes.ExtractFromRight(12, 12)),
        ParseCoordAxis((ushort) fixedPointCoordAxes.ExtractFromRight(0, 12))
    );
}