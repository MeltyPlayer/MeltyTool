using System.Runtime.CompilerServices;

using fin.model;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public int ActiveTexture { get; set; }

  public int[] CurrentTextureBindings { get; }
    = Enumerable.Repeat(0, MaterialConstants.MAX_TEXTURES + 1).ToArray();
}

public static partial class GlUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void BindTexture(int textureIndex, int value) {
    if (currentState_.CurrentTextureBindings[textureIndex] == value) {
      return;
    }

    if (currentState_.ActiveTexture != textureIndex) {
      GL.ActiveTexture(TextureUnit.Texture0 +
                       (currentState_.ActiveTexture = textureIndex));
    }

    GL.BindTexture(TextureTarget.Texture2D,
                   currentState_.CurrentTextureBindings[textureIndex]
                       = value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void UnbindTexture(int textureIndex)
    => BindTexture(textureIndex, 0);
}