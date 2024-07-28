using System;
using System.Collections.Generic;

using Avalonia.Controls;

using fin.animation;
using fin.model;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.resources.model;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.animation {
  public class AnimationsPanelViewModelForDesigner
      : AnimationsPanelViewModel {
    public AnimationsPanelViewModelForDesigner() {
      this.AnimationPlaybackManager = new FrameAdvancer {
          LoopPlayback = true,
      };
      this.Animations = ModelDesignerUtil.CreateStubModel()
                                         .AnimationManager.Animations;
    }
  }

  public class KeyValuePairViewModel(string key, string? value)
      : ViewModelBase {
    public string Key => key;
    public string? Value => value;

    public static implicit operator KeyValuePairViewModel(
        (string key, object? value) tuple)
      => new(tuple.key, tuple.value?.ToString());
  }

  public class AnimationsPanelViewModel : ViewModelBase {
    private IReadOnlyList<IReadOnlyAnimation>? animations_;
    private AnimationListViewModel animationListViewModel_;
    private AnimationViewModel? selectedAnimationViewModel_;

    public IReadOnlyList<IReadOnlyAnimation>? Animations {
      get => this.animations_;
      set {
        this.RaiseAndSetIfChanged(ref this.animations_, value);
        this.AnimationList = new AnimationListViewModel { Animations = value };
      }
    }

    public AnimationListViewModel AnimationList {
      get => this.animationListViewModel_;
      private set
        => this.RaiseAndSetIfChanged(ref this.animationListViewModel_, value);
    }

    public AnimationViewModel? SelectedAnimation {
      get => this.selectedAnimationViewModel_;
      set {
        this.RaiseAndSetIfChanged(ref this.selectedAnimationViewModel_,
                                  value);

        var animationPlaybackManager = this.AnimationPlaybackManager;
        var animation = value?.Animation;
        if (animationPlaybackManager == null || animation == null) {
          return;
        }

        animationPlaybackManager.SetAnimation(animation);

        this.OnAnimationSelected?.Invoke(this, animation);
      }
    }

    public event EventHandler<IReadOnlyAnimation> OnAnimationSelected;

    public AnimationPlaybackPanelViewModel AnimationPlaybackPanel { get; }
      = new();

    public IAnimationPlaybackManager? AnimationPlaybackManager {
      get => this.AnimationPlaybackPanel.AnimationPlaybackManager;
      set => this.AnimationPlaybackPanel.AnimationPlaybackManager = value;
    }
  }

  public partial class AnimationsPanel : UserControl {
    public AnimationsPanel() {
      this.InitializeComponent();
    }

    protected AnimationsPanelViewModel ViewModel
      => Asserts.AsA<AnimationsPanelViewModel>(this.DataContext);

    protected void AnimationList_OnAnimationSelected(
        object? sender,
        AnimationSelectedEventArgs e) {
      this.ViewModel.SelectedAnimation = e.Animation;
    }
  }
}