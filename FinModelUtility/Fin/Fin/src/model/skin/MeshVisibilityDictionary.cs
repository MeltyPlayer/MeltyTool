using System.Collections.Generic;
using System.Linq;

using fin.data.indexable;
using fin.data.queues;

using readOnly;


namespace fin.model.skin;

[GenerateReadOnly]
public partial interface IMeshVisibilityDictionary {
  void Reset();

  bool this[IReadOnlyMesh mesh] { get; set; }
}

public sealed class MeshVisibilityDictionary
    : IMeshVisibilityDictionary {
  private readonly VisibilityNode rootVisibilityNode_;
  private IndexableDictionary<IReadOnlyMesh, VisibilityNode> impl_;

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

  public void Reset() => this.rootVisibilityNode_.Reset();

  public bool this[IReadOnlyMesh mesh] {
    get => this.impl_[mesh].IsVisible;
    set => this.impl_[mesh].LocalVisibility = value;
  }

  private sealed class VisibilityNode(
      int childCount,
      bool defaultLocalVisibility,
      bool defaultInheritedVisibility) {
    private bool inheritedVisibility_ = defaultInheritedVisibility;
    private bool localVisibility_ = defaultLocalVisibility;
    private readonly VisibilityNode[] children_ = new VisibilityNode[childCount];

    public bool IsVisible => this.inheritedVisibility_ && this.LocalVisibility;

    public bool LocalVisibility {
      get => this.localVisibility_;
      set {
        this.localVisibility_ = value;
        this.SetInheritedVisibility_(this.IsVisible);
      }
    }

    public void Reset() {
      this.inheritedVisibility_ = defaultInheritedVisibility;
      this.localVisibility_ = defaultLocalVisibility;

      foreach (var child in this.children_) {
        child.Reset();
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
      foreach (var child in this.children_) {
        child.SetInheritedVisibility_(this.IsVisible);
      }
    }
  }
}