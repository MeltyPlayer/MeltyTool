using System.Drawing;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public Color ClearColor { get; set; }
}

public static partial class GlUtil {
  public static void ClearColorAndDepth() {
      GlUtil.ResetDepth();
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

  public static void ResetClearColor()
    => GlUtil.SetClearColor(Color.FromArgb(51, 128, 179));

  public static void SetClearColor(Color color) {
      if (GlUtil.currentState_.ClearColor == color) {
        return;
      }

      GlUtil.currentState_.ClearColor = color;
      GL.ClearColor(color);
    }
}