using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.model;
using fin.util.asserts;

using Material.Icons;

using NaturalSort.Extension;

using ReactiveUI;

using uni.ui.avalonia.resources.model;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.animation {
  public class AnimationListViewModelForDesigner
      : AnimationListViewModel {
    public AnimationListViewModelForDesigner() {
      this.Animations = ModelDesignerUtil.CreateStubModel()
                                         .AnimationManager
                                         .Animations;
    }
  }

  public class AnimationListViewModel : ViewModelBase {
    private IReadOnlyList<IReadOnlyAnimation>? animations_;
    private ObservableCollection<AnimationViewModel> animationViewModels_;
    private AnimationViewModel? selectedAnimationViewModel_;

    public required IReadOnlyList<IReadOnlyAnimation>? Animations {
      get => this.animations_;
      set {
        this.RaiseAndSetIfChanged(ref this.animations_, value);
        this.AnimationViewModels = new ObservableCollection<AnimationViewModel>(
            value?.Select(a => new AnimationViewModel
                              { Animation = a })
                 .OrderBy(
                     a => a.Animation.Name,
                     new NaturalSortComparer(
                         StringComparison.OrdinalIgnoreCase)) ??
            Enumerable.Empty<AnimationViewModel>());
      }
    }

    public ObservableCollection<AnimationViewModel> AnimationViewModels {
      get => this.animationViewModels_;
      private set {
        this.RaiseAndSetIfChanged(ref this.animationViewModels_, value);
        this.SelectedAnimationViewModel
            = this.AnimationViewModels.FirstOrDefault();
      }
    }

    public AnimationViewModel? SelectedAnimationViewModel {
      get => this.selectedAnimationViewModel_;
      set => this.RaiseAndSetIfChanged(
          ref this.selectedAnimationViewModel_,
          value);
    }
  }

  public class AnimationViewModel : ViewModelBase {
    private IReadOnlyAnimation animation_;
    private MaterialIconKind icon_;

    public required IReadOnlyAnimation Animation {
      get => this.animation_;
      set {
        this.RaiseAndSetIfChanged(ref this.animation_, value);

        var frameCount = value.FrameCount;
        this.Icon = frameCount switch {
            > 1 => MaterialIconKind.AnimationOutline,
            1   => MaterialIconKind.Numeric1BoxOutline,
            0   => MaterialIconKind.Numeric0BoxOutline,
        };
      }
    }

    public MaterialIconKind Icon {
      get => this.icon_;
      private set => this.RaiseAndSetIfChanged(ref this.icon_, value);
    }
  }

  public partial class AnimationList : UserControl {
    public AnimationList() {
      this.InitializeComponent();
    }

    public static readonly RoutedEvent<AnimationSelectedEventArgs>
        AnimationSelectedEvent =
            RoutedEvent.Register<AnimationList, AnimationSelectedEventArgs>(
                nameof(AnimationSelected),
                RoutingStrategies.Direct);

    public event EventHandler<AnimationSelectedEventArgs> AnimationSelected {
      add => this.AddHandler(AnimationSelectedEvent, value);
      remove => this.RemoveHandler(AnimationSelectedEvent, value);
    }

    protected void SelectingItemsControl_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e) {
      if (e.AddedItems.Count == 0) {
        return;
      }

      var selectedAnimationViewModel
          = Asserts.AsA<AnimationViewModel>(e.AddedItems[0]);
      this.RaiseEvent(new AnimationSelectedEventArgs {
          RoutedEvent = AnimationSelectedEvent,
          Animation = selectedAnimationViewModel
      });
    }
  }

  public class AnimationSelectedEventArgs : RoutedEventArgs {
    public required AnimationViewModel Animation { get; init; }
  }
}