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
    this.rootVisibilityNode_ = new VisibilityNode(true, true);
    this.impl_ = new(model.Skin.Meshes.Count);

    var meshQueue = new FinTuple2Queue<IReadOnlyMesh, VisibilityNode>(
        model.Skin.RootMeshes.Select(m => (m, this.rootVisibilityNode_.AddChild(
                                               m.DefaultDisplayState))));
    while (meshQueue.TryDequeue(out var mesh, out var node)) {
      this.impl_[mesh] = node;
      meshQueue.Enqueue(mesh.SubMeshes.Select(m => (m, node.AddChild(
                                                        m.DefaultDisplayState))));
    }
  }

  public void Reset() => this.rootVisibilityNode_.Reset();

  public bool this[IReadOnlyMesh mesh] {
    get => this.impl_[mesh].IsVisible;
    set => this.impl_[mesh].LocalVisibility = value;
  }

  private sealed class VisibilityNode(
      bool defaultLocalVisibility,
      bool defaultInheritedVisibility) {
    private bool inheritedVisibility_ = defaultInheritedVisibility;
    private bool localVisibility_ = defaultLocalVisibility;
    private List<VisibilityNode> children_ = new();

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

    public VisibilityNode AddChild(MeshDisplayState childDefaultDisplayState) {
      var child = new VisibilityNode(
          childDefaultDisplayState is not MeshDisplayState.HIDDEN,
          defaultLocalVisibility &&
          defaultInheritedVisibility);
      children_.Add(child);
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