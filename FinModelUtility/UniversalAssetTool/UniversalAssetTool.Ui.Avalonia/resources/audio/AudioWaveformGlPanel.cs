using Avalonia;

using fin.audio;
using fin.ui.avalonia.gl;
using fin.ui.rendering.gl;

using OpenTK.Graphics.ES30;

namespace uni.ui.avalonia.resources.audio;

public class AudioWaveformGlPanel : BOpenTkControl {
  private readonly AotWaveformRenderer waveformRenderer_ = new();

  public static readonly DirectProperty<AudioWaveformGlPanel,
          IAotAudioPlayback<short>?>
      ActivePlaybackProperty
          = AvaloniaProperty
              .RegisterDirect<AudioWaveformGlPanel, IAotAudioPlayback<short>?>(
                  nameof(ActivePlayback),
                  o => o.waveformRenderer_.ActivePlayback,
                  (o, value) => o.waveformRenderer_.ActivePlayback = value);

  public IAotAudioPlayback<short>? ActivePlayback {
    get => this.GetValue(ActivePlaybackProperty);
    set => this.SetValue(ActivePlaybackProperty, value);
  }

  protected override void InitGl() => GlUtil.ResetGl();
  protected override void TeardownGl() { }

  protected override void RenderGl() {
    this.GetBoundsForGlViewport(out var width, out var height);
    GL.Viewport(0, 0, width, height);

    GL.ClearColor(0, 0, 0, 0);
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    {
      GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
      GlTransform.LoadIdentity();
      GlTransform.Ortho2d(0, width, height, 0);

      GlTransform.MatrixMode(TransformMatrixMode.VIEW);
      GlTransform.LoadIdentity();

      GlTransform.MatrixMode(TransformMatrixMode.MODEL);
      GlTransform.LoadIdentity();
    }

    var amplitude = height * .45f;
    this.waveformRenderer_.Width = width;
    this.waveformRenderer_.Amplitude = amplitude;
    this.waveformRenderer_.MiddleY = height / 2f;
    this.waveformRenderer_.Render();
  }
}