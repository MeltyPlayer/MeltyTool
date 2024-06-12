using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.animation;
using fin.math;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.model;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.animations {
  public class AnimationPlaybackPanelViewModelForDesigner
      : AnimationPlaybackPanelViewModel {
    public AnimationPlaybackPanelViewModelForDesigner() {
      var animation = ModelDesignerUtil.CreateStubAnimation();
      this.AnimationPlaybackManager = new FrameAdvancer {
          FrameRate = (int) animation.FrameRate,
          LoopPlayback = true,
          TotalFrames = animation.FrameCount,
      };
    }
  }

  public class AnimationPlaybackPanelViewModel : ViewModelBase {
    private IAnimationPlaybackManager? animationPlaybackManager_;

    private bool isPlaying_;
    private string playButtonTooltip_;

    private bool loopPlayback_;
    private string loopButtonTooltip_;

    private int frameRate_;
    private float frame_;
    private int frameCount_;
    private float lastFrame_;

    public IAnimationPlaybackManager? AnimationPlaybackManager {
      get => this.animationPlaybackManager_;
      set {
        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.OnUpdate -= this.Update_;
        }

        this.RaiseAndSetIfChanged(
            ref this.animationPlaybackManager_,
            value);
        this.Update_();

        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.OnUpdate += this.Update_;
        }
      }
    }

    public bool IsPlaying {
      get => this.isPlaying_;
      set {
        this.RaiseAndSetIfChanged(ref this.isPlaying_, value);
        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.IsPlaying = value;
        }

        this.PlayButtonTooltip = value ? "Playing" : "Paused";
      }
    }

    public string PlayButtonTooltip {
      get => this.playButtonTooltip_;
      set => this.RaiseAndSetIfChanged(ref this.playButtonTooltip_, value);
    }

    public bool LoopPlayback {
      get => this.loopPlayback_;
      set {
        this.RaiseAndSetIfChanged(ref this.loopPlayback_, value);
        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.LoopPlayback = value;
        }

        this.LoopButtonTooltip = value ? "Looping" : "Not looping";
      }
    }

    public string LoopButtonTooltip {
      get => this.loopButtonTooltip_;
      set => this.RaiseAndSetIfChanged(ref this.loopButtonTooltip_, value);
    }

    public int FrameRate {
      get => this.frameRate_;
      set {
        this.RaiseAndSetIfChanged(ref this.frameRate_, value);
        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.FrameRate = value;
        }
      }
    }

    public float Frame {
      get => this.frame_;
      set {
        this.RaiseAndSetIfChanged(ref this.frame_, value);
        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.Frame = value;
        }
      }
    }

    public int FrameCount {
      get => this.frameCount_;
      private set {
        this.RaiseAndSetIfChanged(ref this.frameCount_, value);
        this.LastFrame = Math.Max(0, value - .0001f);
      }
    }

    public float LastFrame {
      get => this.lastFrame_;
      private set => this.RaiseAndSetIfChanged(ref this.lastFrame_, value);
    }

    private void Update_() {
      var animationPlaybackManager = this.animationPlaybackManager_;
      if (animationPlaybackManager == null) {
        return;
      }

      this.IsPlaying = animationPlaybackManager.IsPlaying;
      this.LoopPlayback = animationPlaybackManager.LoopPlayback;
      this.FrameRate = animationPlaybackManager.FrameRate;
      this.Frame = (float) animationPlaybackManager.Frame;
      this.FrameCount = animationPlaybackManager.TotalFrames;
    }
  }

  public partial class AnimationPlaybackPanel : UserControl {
    public AnimationPlaybackPanel() {
      InitializeComponent();
    }

    private AnimationPlaybackPanelViewModel ViewModel
      => this.DataContext.AssertAsA<AnimationPlaybackPanelViewModel>();

    private void JumpToFirstFrame_(object? sender, RoutedEventArgs e)
      => this.SetFrame_(0);

    private void JumpToPreviousFrame_(object? sender, RoutedEventArgs e)
      => this.SetFrame_(this.ViewModel.Frame - 1);

    private void JumpToNextFrame_(object? sender, RoutedEventArgs e)
      => this.SetFrame_(this.ViewModel.Frame + 1);

    private void JumpToLastFrame_(object? sender, RoutedEventArgs e)
      => this.SetFrame_(this.ViewModel.LastFrame);

    private void SetFrame_(float frame) {
      var viewModel = this.ViewModel;
      viewModel.IsPlaying = false;
      viewModel.Frame = viewModel.LoopPlayback
          ? frame.Wrap(0, viewModel.LastFrame)
          : frame.Clamp(0, viewModel.LastFrame);
    }
  }
}