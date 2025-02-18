namespace vrml.schema;

public interface IGroupNode : INode {
  IReadOnlyList<INode> Children { get; }
}

public interface ITransformNode : IGroupNode, ITransform;

public interface IAnchorNode : IGroupNode {
  string Url { get; }
  string Description { get; }
  IReadOnlyList<string>? Parameter { get; }
}

public record CollisionNode(IReadOnlyList<INode> Children)
    : BNode, IGroupNode;