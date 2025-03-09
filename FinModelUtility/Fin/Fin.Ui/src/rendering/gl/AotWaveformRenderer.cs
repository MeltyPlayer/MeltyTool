using System.Drawing;

using fin.audio;
using fin.math;
using fin.model.impl;
using fin.ui.rendering.gl.model;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public class AotWaveformRenderer {
  private IReadOnlyList<NormalTangentMultiColorMultiUvVertexImpl> vertices_;
  private IDynamicModelRenderer renderer_;

  public IAotAudioPlayback<short>? ActivePlayback { get; set; }

  public int Width { get; set; }
  public float MiddleY { get; set; }
  public float Amplitude { get; set; }

  public void Render() {
    if (this.ActivePlayback?.IsDisposed ?? true) {
      return;
    }

    if (this.vertices_ == null) {
      var model = ModelImpl.CreateForViewer(1000);

      var material = model.MaterialManager.AddColorMaterial(Color.Red);

      var skin = model.Skin;

      this.vertices_ = skin.TypedVertices;
      skin.AddMesh().AddLineStrip(this.vertices_).SetMaterial(material);

      this.renderer_ = ModelRenderer.CreateDynamic(model);
    }

    var source = this.ActivePlayback.TypedSource;

    var samplesPerPoint = 10;
    var xPerPoint = 1;
    var pointCount = Math.Min(this.Width / xPerPoint, this.vertices_.Count - 1);

    var samplesAcrossWidth = samplesPerPoint * pointCount;
    var baseSampleOffset
        = this.ActivePlayback.SampleOffset - samplesAcrossWidth / 2;

    var channelCount = source.AudioChannelsType == AudioChannelsType.STEREO
        ? 2
        : 1;

    for (var i = 0; i <= pointCount; ++i) {
      var fraction = MathF.Sin(MathF.PI * (1f * i / pointCount));

      float totalSample = 0;
      for (var s = 0; s < samplesPerPoint; ++s) {
        var sampleOffset = baseSampleOffset + i * samplesPerPoint + s;
        sampleOffset = sampleOffset.ModRange(0, source.LengthInSamples);

        while (sampleOffset >= source.LengthInSamples) {
          sampleOffset -= source.LengthInSamples;
        }

        for (var c = 0; c < channelCount; ++c) {
          var sample = source.GetPcm(AudioChannelType.STEREO_LEFT + c,
                                     sampleOffset);
          totalSample += sample;
        }
      }

      var meanSample = totalSample / (samplesPerPoint * channelCount);

      float shortMin = short.MinValue;
      float shortMax = short.MaxValue;

      var normalizedShortSample =
          (meanSample - shortMin) / (shortMax - shortMin);

      var floatMin = -1f;
      var floatMax = 1f;

      var floatSample =
          floatMin + normalizedShortSample * (floatMax - floatMin);

      var x = i * xPerPoint;
      var y = this.MiddleY +
              this.Amplitude *
              fraction *
              MathF.Sign(floatSample) *
              MathF.Pow(MathF.Abs(floatSample), .8f);
      this.vertices_[i].SetLocalPosition(x, y, 0);
    }

    if (pointCount + 1 < this.vertices_.Count) {
      var lastY = this.vertices_[pointCount].LocalPosition.Y;

      for (var i = pointCount + 1; i < this.vertices_.Count; ++i) {
        this.vertices_[i].SetLocalPosition(this.Width, lastY, 0);
      }
    }

    GL.LineWidth(1);
    this.renderer_.UpdateBuffer();
    this.renderer_.Render();
  }
}