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
    var faceModel = ModelImpl.CreateForViewer();

    var baseFaceImage = baseFaceTexture.Image;

    var (faceMaterial, faceTexture)
        = faceModel.MaterialManager.AddSimpleTextureMaterialFromImage(
            baseFaceImage);
    faceMaterial.CullingMode = CullingMode.SHOW_BOTH;
    faceTexture.WrapModeU = faceTexture.WrapModeV = WrapMode.CLAMP;

    var faceSkin = faceModel.Skin;
    var pinVertices = new Grid<IVertex>(Expression.WIDTH, Expression.HEIGHT);
    for (var yI = 0; yI < Expression.HEIGHT; ++yI) {
      var v = 1f * yI / (Expression.HEIGHT - 1);
      var y = v * baseFaceImage.Height;

      for (var xI = 0; xI < Expression.WIDTH; ++xI) {
        var u = 1f * xI / (Expression.WIDTH - 1);
        var x = u * baseFaceImage.Width;

        var pinVertex = faceSkin.AddVertex(new Vector3(x, y, 0));
        pinVertex.SetUv(u, v);

        pinVertices[xI, yI] = pinVertex;
      }
    }

    var triangleVertices
        = new List<(IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)>();
    for (var vY = 0; vY < Expression.HEIGHT - 1; ++vY) {
      for (var vX = 0; vX < Expression.WIDTH - 1; ++vX) {
        var a = pinVertices[vX, vY];
        var b = pinVertices[vX + 1, vY];
        var c = pinVertices[vX, vY + 1];
        var d = pinVertices[vX + 1, vY + 1];

        triangleVertices.Add((a, b, c));
        triangleVertices.Add((d, c, b));
      }
    }

    faceSkin.AddMesh()
            .AddTriangles(triangleVertices)
            .SetMaterial(faceMaterial);

    using var faceRenderer = ModelRenderer.CreateDynamic(faceModel);

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

      for (var xI = 0; xI < Expression.WIDTH; ++xI) {
        for (var yI = 0; yI < Expression.HEIGHT; ++yI) {
          var pin = expression.Pins[xI * Expression.HEIGHT + yI];

          pin = (pin - new Vector2(88, 24)) * conversionFactor;

          pinVertices[xI, yI].SetLocalPosition(pin.X, pin.Y, 0);
        }
      }

      fbo.TargetFbo();
      GlUtil.SetViewport(new Rectangle(0, 0, fbo.Width, fbo.Height));
      GlUtil.ClearColorAndDepth();

      faceRenderer.UpdateBuffer();

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