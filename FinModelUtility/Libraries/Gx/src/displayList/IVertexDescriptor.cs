namespace gx.displayList;

public interface IVertexDescriptor
    : IEnumerable<(GxVertexAttribute, GxAttributeType?)> {
  public uint Value { get; set; }
}