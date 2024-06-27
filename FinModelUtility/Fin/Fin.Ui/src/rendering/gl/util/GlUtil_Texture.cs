using System.Runtime.CompilerServices;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public int ActiveTexture { get; set; }= -1;

  public int[] CurrentTextureBindings { get; set; } =
    [-1, -1, -1, -1, -1, -1, -1, -1];
}

public static partial class GlUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void BindTexture(int textureIndex, int value) {
      if (GlUtil.currentState_.CurrentTextureBindings[textureIndex] == value) {
        return;
      }

      if (GlUtil.currentState_.ActiveTexture != textureIndex) {
        GL.ActiveTexture(TextureUnit.Texture0 +
                         (GlUtil.currentState_.ActiveTexture = textureIndex));
      }

      GL.BindTexture(TextureTarget.Texture2D,
                     GlUtil.currentState_.CurrentTextureBindings[textureIndex] = value);
    }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void UnbindTexture(int textureIndex)
    => BindTexture(textureIndex, -1);
}