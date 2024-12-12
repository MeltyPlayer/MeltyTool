using System.Drawing;

using fin.color;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering.gl.model;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public class GridRenderer {
  private IModelRenderer? impl_;

  public float Spacing { get; } = 32;
  public float Size = 1024;

  public void Render() {
    this.impl_ ??= this.GenerateModel_();

    GL.LineWidth(1);
    this.impl_.Render();
  }

  private IModelRenderer GenerateModel_() {
    var model = ModelImpl.CreateForViewer();
    var skin = model.Skin;
    var mesh = skin.AddMesh();

    var size = this.Size;
    var spacing = this.Spacing;

    for (var y = 0f; y <= size / 2; y += spacing) {
      IColor color;
      if (y == 0) {
        color = FinColor.FromRgbFloats(1, 0, 0);
      } else {
        color = FinColor.FromRgbFloats(1, 1, 1);

        var v1Negative = skin.AddVertex(-size / 2, -y, 0);
        v1Negative.SetColor(color);

        var v2Negative = skin.AddVertex(size / 2, -y, 0);
        v2Negative.SetColor(color);

        mesh.AddLines([v1Negative, v2Negative]);
      }

      var v1Positive = skin.AddVertex(-size / 2, y, 0);
      v1Positive.SetColor(color);

      var v2Positive = skin.AddVertex(size / 2, y, 0);
      v2Positive.SetColor(color);

      mesh.AddLines([v1Positive, v2Positive]);
    }

    for (var x = 0f; x <= size / 2; x += spacing) {
      IColor color;
      if (x == 0) {
        color = FinColor.FromRgbFloats(0, 1, 0);
      } else {
        color = FinColor.FromRgbFloats(1, 1, 1);

        var v1Negative = skin.AddVertex(-x, -size / 2, 0);
        v1Negative.SetColor(color);

        var v2Negative = skin.AddVertex(-x, size / 2, 0);
        v2Negative.SetColor(color);

        mesh.AddLines([v1Negative, v2Negative]);
      }

      var v1Positive = skin.AddVertex(x, -size / 2, 0);
      v1Positive.SetColor(color);

      var v2Positive = skin.AddVertex(x, size / 2, 0);
      v2Positive.SetColor(color);

      mesh.AddLines([v1Positive, v2Positive]);
    }

    return new ModelRendererV2(model);
  }
}