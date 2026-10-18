using System.Drawing;

using fin.audio;
using fin.math;
using fin.model.impl;
using fin.ui.rendering.gl.model;

using MathNet.Numerics.IntegralTransforms;


namespace fin.ui.rendering.gl;

public sealed class AotFftRenderer : IRenderable {
  private IReadOnlyList<NormalTangentMultiColorMultiUvVertexImpl> vertices_;
  private IDynamicModelRenderer renderer_;

  public IAotAudioPlayback<short>? ActivePlayback { get; set; }

  private const int POINT_COUNT = 1000;
  private const int SAMPLES_PER_POINT = 10;

  private float[] samples_ = new float[POINT_COUNT * SAMPLES_PER_POINT];

  public int Width { get; set; }
  public float MiddleY { get; set; }
  public float Amplitude { get; set; }

  ~AotFftRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.renderer_.Dispose();

  public void Render() {
    if (this.ActivePlayback?.IsDisposed ?? true) {
      return;
    }

    if (this.vertices_ == null) {
      var model = ModelImpl.CreateForViewer(1000);

      var material = model.MaterialManager.AddColorMaterial(Color.Red);
      var skin = model.Skin;

      this.vertices_ = skin.TypedVertices;
      var lineStrip = skin.AddMesh().AddLineStrip(this.vertices_);
      lineStrip.SetMaterial(material);
      lineStrip.SetLineWidth(1f);

      this.renderer_ = ModelRenderer.CreateDynamic(model);
    }

    var source = this.ActivePlayback.TypedSource;

    var xPerPoint = 1;
    var pointCount = Math.Min(this.Width / xPerPoint, this.vertices_.Count - 1);

    var baseSampleOffset
        = this.ActivePlayback.SampleOffset - this.samples_.Length / 2;

    var channelCount = source.AudioChannelsType == AudioChannelsType.STEREO
        ? 2
        : 1;

    for (var i = 0; i < this.samples_.Length; ++i) {
      var sampleOffset = baseSampleOffset + i;

      sampleOffset = sampleOffset.Mod0ToMax(source.LengthInSamples);

      while (sampleOffset >= source.LengthInSamples) {
        sampleOffset -= source.LengthInSamples;
      }

      var totalSample = 0;
      for (var c = 0; c < channelCount; ++c) {
        var sample = source.GetPcm(AudioChannelType.STEREO_LEFT + c,
                                   sampleOffset);
        totalSample += sample;
      }

      var meanSample = 1f * totalSample / channelCount;

      float shortMin = short.MinValue;
      float shortMax = short.MaxValue;

      var normalizedShortSample =
          (meanSample - shortMin) / (shortMax - shortMin);

      this.samples_[i] = normalizedShortSample;
    }

    Fourier.ForwardReal(this.samples_, this.samples_.Length - 2);

    var graphSamplesPerPoint = (this.samples_.Length - 2) / pointCount;

    for (var i = 0; i <= pointCount; ++i) {
      float totalSample = 0;
      for (var s = 0; s < graphSamplesPerPoint; ++s) {
        var sampleOffset = i * graphSamplesPerPoint + s;
        sampleOffset = sampleOffset.ModRange(0, this.samples_.Length);

        while (sampleOffset >= this.samples_.Length) {
          sampleOffset -= this.samples_.Length;
        }

        var sample = this.samples_[sampleOffset];
        totalSample += sample;
      }
      
      var meanSample = totalSample;
      var floatSample = meanSample;

      var x = i * xPerPoint;
      var y = this.MiddleY +
              this.Amplitude *
              MathF.Sign(floatSample) *
              MathF.Pow(MathF.Abs(floatSample), .8f);
      this.vertices_[i].SetLocalPosition(x, 2 * this.MiddleY - y, 0);
    }

    if (pointCount + 1 < this.vertices_.Count) {
      var lastY = this.vertices_[pointCount].LocalPosition.Y;

      for (var i = pointCount + 1; i < this.vertices_.Count; ++i) {
        this.vertices_[i].SetLocalPosition(this.Width, lastY, 0);
      }
    }

    this.renderer_.UpdateBuffer();
    this.renderer_.Render();
  }
}