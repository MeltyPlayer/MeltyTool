using System.Collections.Generic;

using fin.data.sets;

namespace fin.data.nodes;

public interface ITreeNode<T> : INode<T> {
  IEnumerable<INode<T>> INode<T>.ParentNodes {
    get {
      if (this.Parent != null) {
        yield return this.Parent;
      }
    }
  }

  ITreeNode<T>? Parent { get; set; }

  IEnumerable<INode<T>> INode<T>.ChildNodes => this.ChildNodes;
  new IEnumerable<ITreeNode<T>> ChildNodes { get; }

  bool AddChild(ITreeNode<T> child);
  bool RemoveChild(ITreeNode<T> child);
}

public sealed class TreeNode<T> : ITreeNode<T> {
  private ITreeNode<T>? parent_;
  private readonly OrderedHashSet<ITreeNode<T>> children_ = [];

  public T Value { get; set; }

  public ITreeNode<T>? Parent {
    get => this.parent_;
    set {
      if (this.parent_ == value) {
        return;
      }

      this.parent_?.RemoveChild(this);

      this.parent_ = value;
      value?.AddChild(this);
    }
  }

  public IEnumerable<ITreeNode<T>> ChildNodes => this.children_;

  public bool AddChild(ITreeNode<T> child) {
    var didAdd = this.children_.Add(child);
    if (didAdd) {
      child.Parent = this;
    }

    return didAdd;
  }

  public bool RemoveChild(ITreeNode<T> child) {
    var didRemove = this.children_.Remove(child);
    if (didRemove) {
      child.Parent = null;
    }

    return didRemove;
  }

  public override string ToString() => this.Value?.ToString() ?? "undefined";
}