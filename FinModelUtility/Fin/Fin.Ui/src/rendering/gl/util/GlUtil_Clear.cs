using System.Drawing;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public Color ClearColor { get; set; }
}

public static partial class GlUtil {
  public static void ClearColorAndDepth() {
      ResetDepth();
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

  public static void ResetClearColor()
    => SetClearColor(Color.FromArgb(51, 128, 179));

  public static void SetClearColor(Color color) {
      if (currentState_.ClearColor == color) {
        return;
      }

      currentState_.ClearColor = color;
      GL.ClearColor(color);
    }
}