using System;
using System.Windows.Forms;

using fin.animation;

namespace uni.ui.winforms.right_panel {
  public partial class AnimationPlaybackPanel : UserControl {
    // TODO: Add tests.
    // TODO: How to right-align frame label?
    // TODO: Fix bug where scrolling trackbar changes time dramatically?

    private bool? wasPlayingBeforeScroll_;

    public AnimationPlaybackPanel() {
      this.InitializeComponent();

      this.playButton_.Click += (sender, args) => { this.IsPlaying = true; };
      this.pauseButton_.Click += (sender, args) => { this.IsPlaying = false; };
      this.loopCheckBox_.Click += (sender, args) => {
        this.LoopPlayback = !this.LoopPlayback;
      };

      this.frameTrackBar_.Scroll += (sender, args) => {
        if (this.wasPlayingBeforeScroll_ == null) {
          this.wasPlayingBeforeScroll_ = this.IsPlaying;
          this.IsPlaying = false;
        }

        this.Frame = this.frameTrackBar_.Value;
      };
      this.frameTrackBar_.MouseCaptureChanged += (sender, args) => {
        if (this.wasPlayingBeforeScroll_ != null) {
          this.IsPlaying = this.wasPlayingBeforeScroll_.Value;
          this.wasPlayingBeforeScroll_ = null;
        }
      };

      this.frameRateSelector_.ValueChanged += (sender, args) => {
        var newFrameRate = (int) Math.Floor(this.frameRateSelector_.Value);
        if (this.FrameRate != newFrameRate) {
          this.FrameRate = newFrameRate;
        }
      };

      this.FrameRate = (int) Math.Floor(this.frameRateSelector_.Value);
    }

    private IAnimationPlaybackManager? impl_;

    public IAnimationPlaybackManager? Impl {
      get => this.impl_;
      set {
        if (this.impl_ != null) {
          this.impl_.OnUpdate -= this.Update_;
        }

        this.impl_ = value;

        if (this.impl_ != null) {
          this.impl_.OnUpdate += this.Update_;
        }
      }
    }

    // TODO: Should carry over changes to form.
    public double Frame {
      get => this.Impl?.Frame ?? 0;
      set {
        if (this.Impl != null) {
          this.Impl.Frame = value;
        }

        if (this.TotalFrames > 0) {
          this.frameTrackBar_.Value = (int) value;
        }
      }
    }

    public int TotalFrames {
      get => this.Impl?.TotalFrames ?? 0;
      set {
        if (this.Impl != null) {
          this.Impl.TotalFrames = value;
        }

        this.frameTrackBar_.Maximum = value;
      }
    }

    public int FrameRate {
      get => this.Impl?.FrameRate ?? 0;
      set {
        if (this.Impl != null) {
          this.Impl.FrameRate = value;
        }

        this.frameRateSelector_.Value = value;
      }
    }


    public bool IsPlaying {
      get => this.Impl?.IsPlaying ?? false;
      set {
        if (this.Impl != null) {
          this.Impl.IsPlaying = value;
        }
      }
    }

    public AnimationInterpolationConfig? Config {
      get => this.Impl?.Config;
      set {
        if (this.Impl != null) {
          this.Impl.Config = value;
        }

        this.loopCheckBox_.Checked = value?.UseLoopingInterpolation ?? false;
      }
    }

    public bool LoopPlayback {
      get => this.Impl?.LoopPlayback ?? false;
      set {
        if (this.Impl != null) {
          this.Impl.LoopPlayback = value;
        }

        this.loopCheckBox_.Checked = value;
      }
    }

    private void Update_() {
      if (this.Impl != null) {
        this.frameTrackBar_.Maximum = this.Impl.TotalFrames;
        this.frameRateSelector_.Value = this.Impl.FrameRate;
        this.loopCheckBox_.Checked = this.Impl.LoopPlayback;
      }

      var elapsedSecondsText = "0.0s";
      var elapsedFramesText = "0/0";

      // TODO: What's with all these extra checks?
      if (this.TotalFrames > 0) {
        var frame = this.Frame;
        var frameIndex = (int) Math.Floor(frame);

        this.frameTrackBar_.Value = frameIndex;

        var seconds = frame / this.FrameRate;
        var secondsInt = (int) Math.Max(0, seconds);
        var secondsDelta = (int) (seconds * 1000 % 1000);

        elapsedSecondsText = secondsInt +
                             "." +
                             secondsDelta +
                             "s";
        elapsedFramesText = frameIndex + "/" + this.TotalFrames;
      }

      this.elapsedSeconds_.Text = elapsedSecondsText;
      this.elapsedFrames_.Text = elapsedFramesText;
    }
  }
}