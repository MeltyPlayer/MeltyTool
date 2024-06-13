using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.model;
using fin.util.asserts;

using ReactiveUI;

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
    private SkeletonNode? rootNode_;

    public required IReadOnlySkeleton? Skeleton {
      get => this.skeleton_;
      set {
        this.RaiseAndSetIfChanged(ref this.skeleton_, value);
        this.RootNode = value != null ? new SkeletonNode(value.Root) : null;
      }
    }

    public SkeletonNode? RootNode {
      get => this.rootNode_;
      private set => this.RaiseAndSetIfChanged(ref this.rootNode_, value);
    }
  }

  public class SkeletonNode(IReadOnlyBone bone) : ViewModelBase {
    private bool isExpanded_ = true;

    public IReadOnlyBone Bone => bone;

    public IReadOnlyList<SkeletonNode> Children { get; }
      = bone.Children.Select(b => new SkeletonNode(b)).ToArray();

    public bool IsExpanded {
      get => this.isExpanded_;
      set => this.RaiseAndSetIfChanged(ref this.isExpanded_, value);
    }
  }

  public partial class SkeletonTree : UserControl {
    public SkeletonTree() {
      InitializeComponent();
    }

    public static readonly RoutedEvent<BoneSelectedEventArgs>
        BoneSelectedEvent =
            RoutedEvent.Register<SkeletonTree, BoneSelectedEventArgs>(
                nameof(BoneSelected),
                RoutingStrategies.Direct);

    public event EventHandler<BoneSelectedEventArgs> BoneSelected {
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
          = Asserts.AsA<SkeletonNode>(e.AddedItems[0]);
      this.RaiseEvent(new BoneSelectedEventArgs {
          RoutedEvent = BoneSelectedEvent,
          Bone = selectedBone.Bone
      });
    }
  }

  public class BoneSelectedEventArgs : RoutedEventArgs {
    public required IReadOnlyBone Bone { get; init; }
  }
}