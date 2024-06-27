namespace dat.schema;

public interface IDatNode;

public interface IDatLinkedListNode<out TSelf> : IDatNode
    where TSelf : IDatLinkedListNode<TSelf> {
  TSelf? NextSibling { get; }
}

public interface IDatTreeNode<out TSelf> : IDatLinkedListNode<TSelf>
    where TSelf : IDatTreeNode<TSelf> {
  TSelf? FirstChild { get; }
}

public static class DatNodeExtensions {
  public static IEnumerable<TNode> GetSelfAndSiblings<TNode>(
      this TNode? node)
      where TNode : IDatLinkedListNode<TNode> {
      if (node == null) {
        yield break;
      }

      var current = node;
      while (current != null) {
        yield return current;
        current = current.NextSibling;
      }
    }

  public static IEnumerable<TNode> GetSelfAndChildrenAndSiblings<TNode>(
      this TNode? root)
      where TNode : IDatTreeNode<TNode> {
      if (root == null) {
        yield break;
      }

      yield return root;

      if (root.FirstChild != null) {
        foreach (var child in root.FirstChild.GetSelfAndChildrenAndSiblings()) {
          yield return child;
        }
      }

      if (root.NextSibling != null) {
        foreach (var sibling in
                 root.NextSibling.GetSelfAndChildrenAndSiblings()) {
          yield return sibling;
        }
      }
    }

  public static IEnumerable<TNode> GetChildren<TNode>(this TNode? root)
      where TNode : IDatTreeNode<TNode>
    => root?.FirstChild?.GetSelfAndSiblings() ?? Enumerable.Empty<TNode>();
}