using System.Drawing;

using fin.model;

using OpenTK.Graphics.OpenGL;

using LogicOp = fin.model.LogicOp;

namespace fin.ui.rendering.gl;

public static partial class GlUtil {
  public static void RenderHighlight(
      Action render,
      Color? highlightColor = null) {
    GlUtil.SetBlendColor(highlightColor ?? Color.FromArgb(180, 255, 255, 255));
    GlUtil.SetBlending(BlendEquation.ADD,
                       BlendFactor.CONST_COLOR,
                       BlendFactor.CONST_ALPHA);
    GlUtil.SetDepth(DepthMode.SKIP_WRITE_TO_DEPTH_BUFFER);
    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    GlUtil.DisableChangingBlending = true;
    GlUtil.DisableChangingDepth = true;

    render();

    GlUtil.DisableChangingBlending = false;
    GlUtil.DisableChangingDepth = false;
    GlUtil.SetBlendColor(Color.White);
    GlUtil.ResetBlending();
    GlUtil.ResetDepth();
    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
  }

  public static void RenderOutline(
      Action render,
      Color? outlineColor = null,
      float lineWidth = 8) {
    GlUtil.SetBlendColor(outlineColor ?? Color.Black);
    GlUtil.SetBlending(BlendEquation.ADD,
                       BlendFactor.ZERO,
                       BlendFactor.CONST_COLOR);
    GlUtil.SetDepth(DepthMode.SKIP_WRITE_TO_DEPTH_BUFFER);
    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
    GL.LineWidth(lineWidth);
    GlUtil.DisableChangingBlending = true;
    GlUtil.DisableChangingDepth = true;

    render();

    GlUtil.DisableChangingBlending = false;
    GlUtil.DisableChangingDepth = false;
    GlUtil.SetBlendColor(Color.White);
    GlUtil.ResetBlending();
    GlUtil.ResetDepth();
    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
  }
}