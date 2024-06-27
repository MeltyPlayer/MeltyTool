using fin.audio;
using fin.math;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public class AotWaveformRenderer {
  private readonly float[] points_ = new float[1000];

  public IAotAudioPlayback<short>? ActiveSound { get; set; }

  public int Width { get; set; }
  public float MiddleY { get; set; }
  public float Amplitude { get; set; }

  public void Render() {
      if (this.ActiveSound == null) {
        return;
      }

      var source = this.ActiveSound.TypedSource;

      GlTransform.PassMatricesIntoGl();

      var samplesPerPoint = 10;
      var xPerPoint = 1;
      var pointCount = Width / xPerPoint;
      var points = this.points_.AsSpan(0, pointCount + 1);

      var samplesAcrossWidth = samplesPerPoint * pointCount;
      var baseSampleOffset
          = this.ActiveSound.SampleOffset - samplesAcrossWidth / 2;

      var channelCount = source.AudioChannelsType == AudioChannelsType.STEREO
          ? 2
          : 1;

      for (var i = 0; i <= pointCount; ++i) {
        var fraction = MathF.Sin(MathF.PI * (1f * i / pointCount));

        float totalSample = 0;
        for (var s = 0; s < samplesPerPoint; ++s) {
          var sampleOffset = baseSampleOffset + i * samplesPerPoint + s;
          sampleOffset = sampleOffset.ModRange(0, source.LengthInSamples);

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

        points[i] = fraction *
                    MathF.Sign(floatSample) *
                    MathF.Pow(MathF.Abs(floatSample), .8f);
      }

      GL.Color3(1f, 0, 0);
      GL.LineWidth(1);

      GL.Begin(PrimitiveType.LineStrip);
      for (var i = 0; i <= pointCount; ++i) {
        var x = i * xPerPoint;
        var y = this.MiddleY + this.Amplitude * points[i];
        GL.Vertex2(x, y);
      }

      GL.End();
    }
}