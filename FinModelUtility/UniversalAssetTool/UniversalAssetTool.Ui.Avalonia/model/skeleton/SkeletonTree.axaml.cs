using System;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.io.bundles;
using fin.model;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.textures;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.model.skeleton {
  public class SkeletonTreeViewModelForDesigner
      : SkeletonTreeViewModel {
    public SkeletonTreeViewModelForDesigner() {
      this.Skeleton = ModelDesignerUtil.CreateStubModel().Skeleton;
    }
  }

  public class SkeletonTreeViewModel : ViewModelBase {
    private IReadOnlySkeleton? skeleton_;

    public required IReadOnlySkeleton? Skeleton {
      get => this.skeleton_;
      set => this.RaiseAndSetIfChanged(ref this.skeleton_, value);
    }
  }

  public partial class SkeletonTree : UserControl {
    public SkeletonTree() {
      InitializeComponent();
    }

    public static readonly RoutedEvent<TextureSelectedEventArgs>
        BoneSelectedEvent =
            RoutedEvent.Register<SkeletonTree, TextureSelectedEventArgs>(
                nameof(BoneSelected),
                RoutingStrategies.Direct);

    public event EventHandler<TextureSelectedEventArgs> BoneSelected {
      add => this.AddHandler(BoneSelectedEvent, value);
      remove => this.RemoveHandler(BoneSelectedEvent, value);
    }

    protected void SelectingItemsControl_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e) {
      if (e.AddedItems.Count == 0) {
        return;
      }

      var selectedBone
          = Asserts.AsA<IReadOnlyBone>(e.AddedItems[0]);
      this.RaiseEvent(new BoneSelectedEventArgs {
          RoutedEvent = BoneSelectedEvent,
          Bone = selectedBone
      });
    }
  }

  public class BoneSelectedEventArgs : RoutedEventArgs {
    public required IReadOnlyBone Bone { get; init; }
  }
}