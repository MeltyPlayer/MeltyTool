using System.Drawing;
using System.Numerics;

using fin.model;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.texture;
using fin.ui.rendering.gl.ubo;

using marioartist.schema.talent_studio.face;

namespace marioartist.api;

public static class TstltExpressionFboRenderer {
  public static void GenerateExpressionTextures(
      GlFbo fbo,
      IReadOnlyTexture baseFaceTexture,
      Expression[] expressions) {
    var baseFaceImage = baseFaceTexture.Image;
    using var faceRenderer = new FaceRenderer(baseFaceImage);

    GlUtil.PushState();

    GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
    GlTransform.PushMatrix();
    GlTransform.LoadIdentity();
    GlTransform.Set(
        Matrix4x4.CreateOrthographicOffCenter(0,
                                              fbo.Width,
                                              0,
                                              fbo.Height,
                                              -1,
                                              1));

    GlTransform.MatrixMode(TransformMatrixMode.VIEW);
    GlTransform.PushMatrix();
    GlTransform.LoadIdentity();

    GlTransform.MatrixMode(TransformMatrixMode.MODEL);
    GlTransform.PushMatrix();
    GlTransform.LoadIdentity();

    using var viewMatricesUbo = new ViewMatricesUbo();
    viewMatricesUbo.UpdateData();
    viewMatricesUbo.Bind();

    for (var i = 0; i < expressions.Length; ++i) {
      var expression = expressions[i];

      fbo.TargetFbo();
      GlUtil.SetViewport(new Rectangle(0, 0, fbo.Width, fbo.Height));
      GlUtil.ClearColorAndDepth();

      faceRenderer.SetExpression(expression);
      faceRenderer.Render();

      fbo.UntargetFbo();
    }

    GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
    GlTransform.PopMatrix();

    GlTransform.MatrixMode(TransformMatrixMode.VIEW);
    GlTransform.PopMatrix();

    GlTransform.MatrixMode(TransformMatrixMode.MODEL);
    GlTransform.PopMatrix();

    GlUtil.PopState();
  }
}