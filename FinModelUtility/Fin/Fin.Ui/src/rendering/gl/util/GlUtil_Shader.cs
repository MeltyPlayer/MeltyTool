using System.Runtime.CompilerServices;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public int CurrentShader { get; set; } = -1;
}

public static partial class GlUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void UseProgram(int shader) {
      if (GlUtil.currentState_.CurrentShader == shader) {
        return;
      }

      GlUtil.currentState_.CurrentShader = shader;
      GL.UseProgram(shader);
    }
}