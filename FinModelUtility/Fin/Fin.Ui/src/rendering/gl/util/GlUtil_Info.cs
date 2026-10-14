using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public static partial class GlUtil {
  public static string GetVendor() {
    try {
      return GL.GetString(StringName.Vendor);
    } catch {
      return "(Vendor)";
    }
  }

  public static string GetRenderer()  {
    try {
      return GL.GetString(StringName.Renderer);
    } catch {
      return "(Renderer)";
    }
  }
}