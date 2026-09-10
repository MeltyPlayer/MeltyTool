using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public int CurrentFboId { get; set; }
}

public static partial class GlUtil {
  public static void ResetFbo() => BindFbo(0);

  public static void BindFbo(int fboId) {
    if (currentState_.CurrentFboId == fboId) {
      return;
    }

    GL.BindFramebuffer(
        FramebufferTarget.Framebuffer,
        currentState_.CurrentFboId = fboId);
  }
}