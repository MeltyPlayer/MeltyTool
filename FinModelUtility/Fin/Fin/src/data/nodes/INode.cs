using System.Collections.Generic;

namespace fin.data.nodes;

public interface INode<out T> {
  T Value { get; }

  /// <summary>
  ///   Nodes that contain a link from themselves to this node.
  /// </summary>
  IEnumerable<INode<T>> ParentNodes { get; }

  /// <summary>
  ///   Nodes where this node contains a link to that other node.
  /// </summary>
  IEnumerable<INode<T>> ChildNodes { get; }
}