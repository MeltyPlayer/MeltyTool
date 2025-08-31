using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public int CurrentVaoId { get; set; } = -1;
}

public static partial class GlUtil {
  public static void BindVao(int vaoId) {
    if (currentState_.CurrentVaoId == vaoId) {
      return;
    }

    GL.BindVertexArray(currentState_.CurrentVaoId = vaoId);
  }
}