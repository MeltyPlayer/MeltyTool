using System.Drawing;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public Rectangle Viewport { get; set; } = Rectangle.Empty;
}

public static partial class GlUtil {
  public static void SetViewport(Rectangle viewport) {
    if (currentState_.Viewport == viewport) {
      return;
    }

    GL.Viewport(viewport);
  }
}