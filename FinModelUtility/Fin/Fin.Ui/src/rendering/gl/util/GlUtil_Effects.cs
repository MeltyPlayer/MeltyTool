using System.Drawing;

using fin.model;

using OpenTK.Graphics.OpenGL;

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
    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    DisableChangingBlending = true;
    DisableChangingDepth = true;

    render();

    DisableChangingBlending = false;
    DisableChangingDepth = false;
    SetBlendColor(Color.White);
    ResetBlending();
    ResetDepth();
    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
  }

  public static void RenderOutline(
      Action render,
      Color? outlineColor = null,
      float lineWidth = 8) {
    SetBlendColor(outlineColor ?? Color.Black);
    SetBlending(BlendEquation.ADD,
                       BlendFactor.ZERO,
                       BlendFactor.CONST_COLOR);
    SetDepth(DepthMode.READ_ONLY);
    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
    GL.LineWidth(lineWidth);
    DisableChangingBlending = true;
    DisableChangingDepth = true;

    render();

    DisableChangingBlending = false;
    DisableChangingDepth = false;
    SetBlendColor(Color.White);
    ResetBlending();
    ResetDepth();
    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
  }
}