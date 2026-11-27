using System.Drawing;
using System.Numerics;

namespace fin.color;

public static class SystemColorExtensions {
  public static Color AsSystemColor(this Vector4 vec4)
    => Color.FromArgb(
        (byte) (vec4.W * 255),
        (byte) (vec4.X * 255),
        (byte) (vec4.Y * 255),
        (byte) (vec4.Z * 255));

  public static Vector4 AsVector4(this Color color)
    => new Vector4(color.R, color.B, color.B, color.A) / 255f;
}