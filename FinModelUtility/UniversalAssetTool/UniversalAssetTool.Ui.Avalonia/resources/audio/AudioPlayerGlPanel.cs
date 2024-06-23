using System;
using System.Collections.Generic;
using System.Drawing;

using fin.audio;
using fin.audio.io;
using fin.data;
using fin.model;
using fin.ui.playback.al;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.material;
using fin.util.time;

using OpenTK.Graphics.OpenGL;

using uni.api;
using uni.ui.avalonia.common.gl;

using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;

namespace uni.ui.avalonia.resources.audio;

public class AudioPlayerGlPanel : BOpenTkControl {
  private IReadOnlyList<IAudioFileBundle>? audioFileBundles_;
  private ShuffledListView<IAudioFileBundle>? shuffledListView_;
  private readonly IAudioManager<short> audioManager_ = new AlAudioManager();
  private readonly IAudioPlayer<short> audioPlayer_;

  private readonly AotWaveformRenderer waveformRenderer_ = new();

  private readonly TimedCallback playNextCallback_;

  public AudioPlayerGlPanel() {
    this.audioPlayer_ = this.audioManager_.AudioPlayer;

    var playNextLock = new object();
    this.playNextCallback_ = new TimedCallback(
        () => {
          lock (playNextLock) {
            if (this.shuffledListView_ == null) {
              return;
            }

            var activeSound = this.waveformRenderer_.ActiveSound;
            if (activeSound?.State == PlaybackState.PLAYING) {
              return;
            }

            this.waveformRenderer_.ActiveSound = null;
            activeSound?.Stop();
            activeSound?.Dispose();

            if (this.shuffledListView_.TryGetNext(out var audioFileBundle)) {
              var audioBuffer = new GlobalAudioReader().ImportAudio(
                  this.audioManager_,
                  audioFileBundle);

              activeSound = this.waveformRenderer_.ActiveSound =
                  this.audioPlayer_.CreatePlayback(audioBuffer);
              activeSound.Volume = .1f;
              activeSound.Play();

              this.OnChange(audioFileBundle);
            }
          }
        },
        .1f);
  }

  /// <summary>
  ///   Sets the audio file bundles to play in the player.
  /// </summary>
  public IReadOnlyList<IAudioFileBundle>? AudioFileBundles {
    get => this.audioFileBundles_;
    set {
      var originalValue = this.audioFileBundles_;
      this.audioFileBundles_ = value;

      this.waveformRenderer_.ActiveSound?.Stop();
      this.waveformRenderer_.ActiveSound = null;

      this.shuffledListView_
          = value != null
              ? new ShuffledListView<IAudioFileBundle>(value)
              : null;

      if (value == null && originalValue != null) {
        this.OnChange(null);
      }
    }
  }

  public event Action<IAudioFileBundle?> OnChange = delegate { };

  protected override void InitGl() => GlUtil.ResetGl();
  protected override void TeardownGl() => this.audioManager_.Dispose();

  protected override void RenderGl() {
    // No idea why this scaling is necessary, it just is.
    var width = (int) (this.Bounds.Width * 1.25);
    var height = (int) (this.Bounds.Height * 1.25);
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

    CommonShaderPrograms.TEXTURELESS_SHADER_PROGRAM.Use();

    var amplitude = height * .45f;
    this.waveformRenderer_.Width = width;
    this.waveformRenderer_.Amplitude = amplitude;
    this.waveformRenderer_.MiddleY = height / 2f;
    this.waveformRenderer_.Render();
  }
}