using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.model;
using fin.ui.rendering;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.model.skeleton {
  public class SkeletonTreeViewModelForDesigner
      : SkeletonTreeViewModel {
    public SkeletonTreeViewModelForDesigner() {
      this.Skeleton = ModelDesignerUtil.CreateStubModel().Skeleton;
    }
  }

  public class SkeletonTreeViewModel : ViewModelBase {
    public required IReadOnlySkeleton? Skeleton {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RootNode = value != null ? new SkeletonNode(value.Root) : null;
      }
    }

    public SkeletonNode? RootNode {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public SkeletonNode? SelectedNode {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }
  }

  public class SkeletonNode(IReadOnlyBone bone) : ViewModelBase {
    public IReadOnlyBone Bone => bone;

    public IReadOnlyList<SkeletonNode> Children { get; }
      = bone.Children.Select(b => new SkeletonNode(b)).ToArray();

    public bool IsExpanded {
      get;
      set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;
  }

  public partial class SkeletonTree : UserControl {
    public SkeletonTree() {
      this.InitializeComponent();
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
        SelectedBoneService.SelectBone(null);
        return;
      }

      var selectedBone
          = Asserts.AsA<SkeletonNode>(e.AddedItems[0]);
      this.RaiseEvent(new BoneSelectedEventArgs {
          RoutedEvent = BoneSelectedEvent,
          Bone = selectedBone.Bone
      });
      SelectedBoneService.SelectBone(selectedBone.Bone);
    }
  }

  public class BoneSelectedEventArgs : RoutedEventArgs {
    public required IReadOnlyBone Bone { get; init; }
  }
}