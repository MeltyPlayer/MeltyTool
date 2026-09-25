using System.Runtime.CompilerServices;

using fin.model;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public int[] CurrentSamplerBindings { get; }
    = Enumerable.Repeat(0, MaterialConstants.MAX_TEXTURES + 1).ToArray();
}

public static partial class GlUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void BindSampler(int samplerIndex, int value) {
    if (currentState_.CurrentSamplerBindings[samplerIndex] == value) {
      return;
    }

    GL.BindSampler(samplerIndex,
                   currentState_.CurrentSamplerBindings[samplerIndex] = value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void UnbindSampler(int samplerIndex)
    => BindSampler(samplerIndex, 0);
}