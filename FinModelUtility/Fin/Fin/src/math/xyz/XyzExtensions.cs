using fin.math.floats;

namespace fin.math.xyz {
  public static class XyzExtensions {
    public static bool IsRoughly0(this IReadOnlyXyz vec3)
      => vec3.X.IsRoughly0() && vec3.Y.IsRoughly0() && vec3.Z.IsRoughly0();

    public static bool IsRoughly1(this IReadOnlyXyz vec3)
      => vec3.X.IsRoughly1() && vec3.Y.IsRoughly1() && vec3.Z.IsRoughly1();
  }
}
