using System.Numerics;

using fin.data;
using fin.image;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering;
using fin.ui.rendering.gl.model;

using marioartist.schema.talent_studio.face;

namespace marioartist.api;

public class FaceRenderer : IRenderable {
  private readonly Grid<IVertex> pinVertices_;
  private readonly IDynamicModelRenderer impl_;
  private readonly Vector2 conversionFactor_;

  public FaceRenderer(IReadOnlyImage baseFaceImage) {
    var faceModel = ModelImpl.CreateForViewer();

    this.conversionFactor_
        = new Vector2(baseFaceImage.Width, baseFaceImage.Height) /
          new Vector2(Expression.WIDTH - 1, Expression.HEIGHT - 1) /
          16;

    var (faceMaterial, faceTexture)
        = faceModel.MaterialManager.AddSimpleTextureMaterialFromImage(
            baseFaceImage);
    faceTexture.WrapModeU = faceTexture.WrapModeV = WrapMode.CLAMP;

    var faceSkin = faceModel.Skin;
    this.pinVertices_ = new Grid<IVertex>(Expression.WIDTH, Expression.HEIGHT);
    for (var yI = 0; yI < Expression.HEIGHT; ++yI) {
      var v = 1f * yI / (Expression.HEIGHT - 1);
      var y = v * baseFaceImage.Height;

      for (var xI = 0; xI < Expression.WIDTH; ++xI) {
        var u = 1f * xI / (Expression.WIDTH - 1);
        var x = u * baseFaceImage.Width;

        var pinVertex = faceSkin.AddVertex(new Vector3(x, y, 0));
        pinVertex.SetUv(u, v);

        this.pinVertices_[xI, yI] = pinVertex;
      }
    }

    var triangleVertices
        = new List<(IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)>();
    for (var vY = 0; vY < Expression.HEIGHT - 1; ++vY) {
      for (var vX = 0; vX < Expression.WIDTH - 1; ++vX) {
        var a = this.pinVertices_[vX, vY];
        var b = this.pinVertices_[vX + 1, vY];
        var c = this.pinVertices_[vX, vY + 1];
        var d = this.pinVertices_[vX + 1, vY + 1];

        triangleVertices.Add((a, b, c));
        triangleVertices.Add((d, c, b));
      }
    }

    faceSkin.AddMesh()
            .AddTriangles(triangleVertices)
            .SetMaterial(faceMaterial);

    this.impl_ = ModelRenderer.CreateDynamic(faceModel);
  }

  ~FaceRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

  public void SetExpression(Expression expression) {
    for (var xI = 0; xI < Expression.WIDTH; ++xI) {
      for (var yI = 0; yI < Expression.HEIGHT; ++yI) {
        var pin = expression.Pins[xI * Expression.HEIGHT + yI];

        pin = (pin - new Vector2(88, 24)) * this.conversionFactor_;

        this.pinVertices_[xI, yI].SetLocalPosition(pin.X, pin.Y, 0);
      }
    }

    this.impl_.UpdateBuffer();
  }

  public void Render() => this.impl_.Render();
}