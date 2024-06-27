using System.Collections.Generic;

using fin.data.sets;

namespace fin.data.nodes;

public interface IGraphNode<T> : INode<T> {
  IEnumerable<INode<T>> INode<T>.ParentNodes => this.ParentNodes;
  new IEnumerable<IGraphNode<T>> ParentNodes { get; }

  IEnumerable<INode<T>> INode<T>.ChildNodes => this.ChildNodes;
  new IEnumerable<IGraphNode<T>> ChildNodes { get; }

  bool AddOneWayLinkTo(IGraphNode<T> child);
  bool AddOneWayLinkFrom(IGraphNode<T> parent);
  bool AddTwoWayLinkTo(IGraphNode<T> other);
}

public sealed class GraphNode<T> : IGraphNode<T> {
  private readonly OrderedHashSet<IGraphNode<T>> parentNodes_ = [];
  private readonly OrderedHashSet<IGraphNode<T>> childNodes_ = [];

  public T Value { get; set; }

  public IEnumerable<IGraphNode<T>> ParentNodes => this.parentNodes_;
  public IEnumerable<IGraphNode<T>> ChildNodes => this.childNodes_;

  public bool AddOneWayLinkTo(IGraphNode<T> child) {
    var didAdd = this.childNodes_.Add(child);
    if (didAdd) {
      child.AddOneWayLinkFrom(this);
    }

    return didAdd;
  }

  public bool AddOneWayLinkFrom(IGraphNode<T> parent) {
    var didAdd = this.parentNodes_.Add(parent);
    if (didAdd) {
      parent.AddOneWayLinkTo(this);
    }

    return didAdd;
  }

  public bool AddTwoWayLinkTo(IGraphNode<T> other) {
    var didAdd = this.AddOneWayLinkTo(other);
    if (didAdd) {
      other.AddOneWayLinkTo(this);
    }

    return didAdd;
  }
}