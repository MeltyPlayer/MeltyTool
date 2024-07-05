using System.Runtime.CompilerServices;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public bool UpdateColorChannel { get; set; } = true;
  public bool UpdateAlphaChannel { get; set; } = true;
}

public static partial class GlUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetChannelUpdateMask(bool updateColorChannel,
                                          bool updateAlphaChannel) {
    if (GlUtil.currentState_.UpdateColorChannel == updateColorChannel &&
        GlUtil.currentState_.UpdateAlphaChannel == updateAlphaChannel) {
      return;
    }

    GlUtil.currentState_.UpdateColorChannel = updateColorChannel;
    GlUtil.currentState_.UpdateAlphaChannel = updateAlphaChannel;
    GL.ColorMask(updateColorChannel,
                 updateColorChannel,
                 updateColorChannel,
                 updateAlphaChannel);
  }
}