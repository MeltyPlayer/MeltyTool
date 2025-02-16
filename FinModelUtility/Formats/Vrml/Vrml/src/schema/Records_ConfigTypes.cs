using fin.model;

namespace vrml.schema;

public record ShapeHintsNode : BNode {
  public string ShapeType { get; init; }
  public VertexOrder VertexOrdering { get; init; }
}