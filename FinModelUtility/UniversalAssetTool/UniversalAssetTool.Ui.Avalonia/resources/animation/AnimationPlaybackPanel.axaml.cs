using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.animation;
using fin.math;
using fin.ui.avalonia;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.resources.model;

namespace uni.ui.avalonia.resources.animation {
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
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);
        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.IsPlaying = value;
        }

        this.PlayButtonTooltip = value ? "Playing" : "Paused";
      }
    }

    public string PlayButtonTooltip {
      get;
      set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool LoopPlayback {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);
        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.LoopPlayback = value;
        }

        this.LoopButtonTooltip = value ? "Looping" : "Not looping";
      }
    }

    public string LoopButtonTooltip {
      get;
      set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int FrameRate {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);
        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.FrameRate = value;
        }
      }
    }

    public float Frame {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);
        if (this.animationPlaybackManager_ != null) {
          this.animationPlaybackManager_.Frame = value;
        }
      }
    }

    public int FrameCount {
      get;
      private set {
        this.RaiseAndSetIfChanged(ref field, value);
        this.LastFrame = Math.Max(0, value - .0001f);

        var digitsInFrameCount = field.Base10DigitCount();
        this.FrameTextWidth = 45 + 10 * digitsInFrameCount;
      }
    }

    public float LastFrame {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int FrameTextWidth {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
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
      this.InitializeComponent();
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