using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public int CurrentVaoId { get; set; } = -1;
}

public static partial class GlUtil {
  public static void BindVao(int vaoId) {
      if (GlUtil.currentState_.CurrentVaoId == vaoId) {
        return;
      }

      GL.BindVertexArray(GlUtil.currentState_.CurrentVaoId = vaoId);
    }
}