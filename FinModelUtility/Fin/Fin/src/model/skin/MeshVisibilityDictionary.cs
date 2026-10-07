using System.Collections.Generic;
using System.Linq;

using fin.data.indexable;
using fin.data.queues;

using readOnly;


namespace fin.model.skin;

[GenerateReadOnly]
public partial interface IMeshVisibilityDictionary {
  void Reset();

  bool this[IReadOnlyMesh mesh] { get; }

  bool AnyUserHidden { get; }

  void SetLocalVisibility(IReadOnlyMesh mesh, bool isVisible);
  void SetUserVisibility(IReadOnlyMesh mesh, bool isVisible);
}

public sealed class MeshVisibilityDictionary
    : IMeshVisibilityDictionary {
  private readonly VisibilityNode rootVisibilityNode_;
  private IndexableDictionary<IReadOnlyMesh, VisibilityNode> impl_;

  private int userHiddenCount_;

  public MeshVisibilityDictionary(IReadOnlyModel model) {
    var rootMeshes = model.Skin.RootMeshes;
    this.rootVisibilityNode_ = new VisibilityNode(rootMeshes.Count, true, true);
    this.impl_ = new(model.Skin.Meshes.Count);

    var meshQueue = new FinTuple3Queue<IReadOnlyMesh, int, VisibilityNode>(
        model.Skin.RootMeshes.Select((m, i) => (m, i, this.rootVisibilityNode_)));
    while (meshQueue.TryDequeue(out var mesh, out var indexInParent, out var parentNode)) {
      var subMeshes = mesh.SubMeshes;

      var node = parentNode.SetChild(indexInParent, subMeshes.Count, mesh.DefaultDisplayState);
      this.impl_[mesh] = node;

      meshQueue.Enqueue(mesh.SubMeshes.Select((m, i) => (m, i, node)));
    }
  }

  public void Reset() {
    this.rootVisibilityNode_.Reset(true);
  }

  public bool this[IReadOnlyMesh mesh] => this.impl_[mesh].IsVisible;

  public bool AnyUserHidden => this.userHiddenCount_ > 0;

  public void SetLocalVisibility(IReadOnlyMesh mesh, bool isVisible) {
    this.impl_[mesh].LocalVisibility = isVisible;
  }

  public void SetUserVisibility(IReadOnlyMesh mesh, bool isVisible) {
    VisibilityNode node = this.impl_[mesh];

    if (node.UserVisibility != isVisible) {
      node.UserVisibility = isVisible;
      this.userHiddenCount_ += isVisible ? -1 : 1;
    }
  }

  private sealed class VisibilityNode(
      int childCount,
      bool defaultLocalVisibility,
      bool defaultInheritedVisibility) {
    private bool inheritedVisibility_ = defaultInheritedVisibility;
    private bool localVisibility_ = defaultLocalVisibility;

    // The user-specified visibility for this node
    private bool userVisibility_ = true;
    private readonly VisibilityNode[] children_ = new VisibilityNode[childCount];

    public bool IsVisible => this.inheritedVisibility_ && this.localVisibility_ && this.userVisibility_;

    public bool UserVisibility {
      get => this.userVisibility_;
      set {
        this.userVisibility_ = value;
        this.UpdateChildrenInheritedVisibility_();
      }
    }

    public bool LocalVisibility {
      get => this.localVisibility_;
      set {
        this.localVisibility_ = value;
        this.UpdateChildrenInheritedVisibility_();
      }
    }

    public void Reset(bool newInheritedVisibility) {
      this.inheritedVisibility_ = newInheritedVisibility;
      this.localVisibility_ = defaultLocalVisibility;

      bool isVisible = this.IsVisible;
      foreach (VisibilityNode child in this.children_) {
        child.Reset(isVisible);
      }
    }

    public VisibilityNode SetChild(int index, int childCount, MeshDisplayState childDefaultDisplayState) {
      var child = new VisibilityNode(
          childCount,
          childDefaultDisplayState is not MeshDisplayState.HIDDEN,
          defaultLocalVisibility &&
          defaultInheritedVisibility);
      this.children_[index] = child;
      return child;
    }

    private void SetInheritedVisibility_(bool inheritedVisibility) {
      if (this.inheritedVisibility_ == inheritedVisibility) {
        return;
      }

      this.inheritedVisibility_ = inheritedVisibility;
      this.UpdateChildrenInheritedVisibility_();
    }

    private void UpdateChildrenInheritedVisibility_() {
      bool isVisible = this.IsVisible;
      foreach (VisibilityNode child in this.children_) {
        child.SetInheritedVisibility_(isVisible);
      }
    }
  }
}