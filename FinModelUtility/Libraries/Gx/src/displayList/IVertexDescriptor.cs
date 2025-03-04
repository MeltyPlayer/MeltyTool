namespace gx.displayList;

public interface IVertexDescriptor
    : IEnumerable<(GxVertexAttribute, GxAttributeType?, GxColorComponentType?)> {
  public uint Value { get; set; }
}