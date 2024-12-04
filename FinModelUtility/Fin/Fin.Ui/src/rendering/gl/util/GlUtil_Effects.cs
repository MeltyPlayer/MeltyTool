using System.Drawing;

using fin.model;

namespace fin.ui.rendering.gl;

public static partial class GlUtil {
  public static void RenderHighlight(
      Action render,
      Color? highlightColor = null) {
    SetBlendColor(highlightColor ?? Color.FromArgb(180, 255, 255, 255));
    SetBlending(BlendEquation.ADD,
                       BlendFactor.CONST_COLOR,
                       BlendFactor.CONST_ALPHA);
    SetDepth(DepthMode.READ_ONLY);
    DisableChangingBlending = true;
    DisableChangingDepth = true;

    render();

    DisableChangingBlending = false;
    DisableChangingDepth = false;
    SetBlendColor(Color.White);
    ResetBlending();
    ResetDepth();
  }
}