using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public int CurrentEboId { get; set; }
}

public static partial class GlUtil {
  public static void ResetEbo() => BindEbo(0);

  public static void BindEbo(int eboId) {
    if (currentState_.CurrentEboId == eboId) {
      return;
    }

    GL.BindBuffer(BufferTarget.ElementArrayBuffer,
                  currentState_.CurrentEboId = eboId);
  }
}