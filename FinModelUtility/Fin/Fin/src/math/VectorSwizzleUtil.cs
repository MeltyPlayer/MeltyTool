using System.Numerics;

using fin.math.xyz;

namespace fin.math;

public static class VectorSwizzleUtil {
  public static Vector2 Xy(this IReadOnlyXyz vec3) => new(vec3.X, vec3.Y);
  public static Vector2 Xy(this Vector4 vec4) => new(vec4.X, vec4.Y);

  public static Vector2 Xz(this Vector3 vec3) => new(vec3.X, vec3.Z);
  public static Vector2 Xz(this Vector4 vec4) => new(vec4.X, vec4.Z);

  public static Vector3 Xyz(this Vector4 vec4) => new(vec4.X, vec4.Y, vec4.Z);

  public static Vector4 Yzwx(this Vector4 vec4)
    => new(vec4.Y, vec4.Z, vec4.W, vec4.X);
}