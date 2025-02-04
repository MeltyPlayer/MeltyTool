using fin.model;

namespace vrml.schema;

public record ShapeHintsNode : INode {
  public string ShapeType { get; init; }
  public VertexOrder VertexOrdering { get; init; }
}