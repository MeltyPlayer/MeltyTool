using System.Drawing;
using System.Numerics;

using fin.animation.keyframes;
using fin.data;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;
using fin.ui.rendering.gl.texture;
using fin.ui.rendering.gl.ubo;

using marioartist.schema.talent_studio.face;

namespace marioartist.api;

public static class TstltExpressionImageGenerator {
  public static void GenerateExpressionTextures(
      IMaterialManager dstMaterialManager,
      IAnimationManager dstAnimationManager,
      IReadOnlyTexture baseFaceTexture,
      Expression[] expressions) {
    var baseFaceImage = baseFaceTexture.Image;
    using var faceRenderer = new FaceRenderer(baseFaceImage);
    using var fbo = new GlFbo(baseFaceImage.Width, baseFaceImage.Height);

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

    var conversionFactor = new Vector2(fbo.Width, fbo.Height) /
                           new Vector2(Expression.WIDTH - 1,
                                       Expression.HEIGHT - 1) /
                           16;

    for (var i = 0; i < expressions.Length; ++i) {
      var expression = expressions[i];
      var expressionName = i switch {
          0 => "normal",
          1 => "laugh",
          2 => "angry",
          3 => "sad",
          4 => "free",
          5 => "sleep",
      };

      fbo.TargetFbo();
      GlUtil.SetViewport(new Rectangle(0, 0, fbo.Width, fbo.Height));
      GlUtil.ClearColorAndDepth();

      faceRenderer.SetExpression(expression);
      faceRenderer.Render();

      fbo.UntargetFbo();

      var expressionImage = fbo.ConvertToImage();
      var expressionTexture
          = dstMaterialManager.CreateTexture(expressionImage);
      expressionTexture.Name = expressionName;

      var expressionAnimation = dstAnimationManager.AddAnimation();
      expressionAnimation.Name = expressionName;

      var textureTracks = expressionAnimation.AddTextureTracks(baseFaceTexture);
      textureTracks.UseFlipbookSwapKeyframes()
                   .SetKeyframe(0, expressionTexture);
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