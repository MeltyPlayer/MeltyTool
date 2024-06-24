using System.Drawing;

using fin.color;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering.gl.model;

namespace fin.ui.rendering.gl {
  public class BackgroundSphereRenderer : IRenderable {
    private IModelRenderer? impl_;

    public void Render() {
      this.impl_ ??= this.GenerateModel_();
      this.impl_.Render();
    }

    private readonly Gradient gradient_ = new(
    [
        (FinColor.FromSystemColor(Color.DarkBlue), 0f),
            (FinColor.FromSystemColor(Color.RoyalBlue), .2f),
            (FinColor.FromSystemColor(Color.LightSkyBlue), .4f),
            (FinColor.FromSystemColor(Color.AliceBlue), .5f),
            (FinColor.FromRgbBytes(66, 52, 49), .5f)
    ]);

    private IModelRenderer GenerateModel_() {
      var model = ModelImpl.CreateForViewer();

      var mesh = model.Skin.AddMesh();

      var material = model.MaterialManager.AddNullMaterial();
      material.DepthMode = DepthMode.SKIP_WRITE_TO_DEPTH_BUFFER;

      var scale = 100;

      var n = 300;
      for (var pitchThetaI = 0; pitchThetaI < n / 2; ++pitchThetaI) {
        var frac0 = 2f * pitchThetaI / n;
        var frac1 = 2f * (pitchThetaI + 1) / n;

        var pitchTheta0 = MathF.PI * frac0;
        var pitchTheta1 = MathF.PI * frac1;

        var zComponent0 = scale * FinTrig.Cos(pitchTheta0);
        var xyComponent0 = scale * FinTrig.Sin(pitchTheta0);
        var color0 = this.gradient_.GetColor(frac0);

        var zComponent1 = scale * FinTrig.Cos(pitchTheta1);
        var xyComponent1 = scale * FinTrig.Sin(pitchTheta1);
        var color1 = this.gradient_.GetColor(frac1);

        var triangles = new List<IVertex>();

        for (var yawThetaI = 0; yawThetaI <= n; ++yawThetaI) {
          var yawTheta = 2 * MathF.PI * yawThetaI / n;

          var xComponent0 = xyComponent0 * FinTrig.Cos(yawTheta);
          var yComponent0 = xyComponent0 * FinTrig.Sin(yawTheta);

          var xComponent1 = xyComponent1 * FinTrig.Cos(yawTheta);
          var yComponent1 = xyComponent1 * FinTrig.Sin(yawTheta);

          var vertex0 =
              model.Skin.AddVertex(xComponent0, yComponent0, zComponent0);
          vertex0.SetColor(color0);

          var vertex1 =
              model.Skin.AddVertex(xComponent1, yComponent1, zComponent1);
          vertex1.SetColor(color1);

          triangles.Add(vertex0);
          triangles.Add(vertex1);
        }

        mesh.AddTriangleStrip(triangles.ToArray()).SetMaterial(material);
      }

      return new ModelRendererV2(model, null);
    }
  }

  public class Gradient {
    private (IColor color, float fraction)[] colorsAndFractions_;

    public Gradient((IColor, float)[] colorsAndFractions) {
      this.colorsAndFractions_ = colorsAndFractions;
    }

    public IColor GetColor(float frac) {
      for (var i = 0; i < this.colorsAndFractions_.Length - 1; ++i) {
        var current = this.colorsAndFractions_[i];
        var next = this.colorsAndFractions_[i + 1];

        if (next.fraction > frac) {
          var subFrac = (frac - current.fraction) /
                        (next.fraction - current.fraction);
          return FinColor.Lerp(current.color, next.color, subFrac);
        }
      }

      return this.colorsAndFractions_[^1].color;
    }
  }
}