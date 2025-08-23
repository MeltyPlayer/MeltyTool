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

  public static bool AreColorFormatAndBitsPerTexelValid(
      N64ColorFormat colorFormat,
      BitsPerTexel bitsPerTexel)
    => (colorFormat, bitsPerTexel) switch {
        (N64ColorFormat.RGBA, BitsPerTexel._16BPT) => true,
        (N64ColorFormat.RGBA, BitsPerTexel._32BPT) => true,
        (N64ColorFormat.L, BitsPerTexel._4BPT)     => true,
        (N64ColorFormat.L, BitsPerTexel._8BPT)     => true,
        (N64ColorFormat.LA, BitsPerTexel._8BPT)    => true,
        (N64ColorFormat.LA, BitsPerTexel._16BPT)   => true,
        (N64ColorFormat.CI, BitsPerTexel._4BPT)    => true,
        (N64ColorFormat.CI, BitsPerTexel._8BPT)    => true,
        _                                          => false,
    };
}