using System.Collections;

using gx;
using gx.displayList;

using schema.binary;

namespace sysdolphin.schema.mesh;

[BinarySchema]
public partial class VertexDescriptors
    : IVertexDescriptor, IBinaryConvertible {
  private readonly Dictionary<GxVertexAttribute, VertexDescriptor> map_ = new();

  private IEnumerable<(GxVertexAttribute, GxAttributeType?, GxColorComponentType
      ?)> cachedEnumerable_;

  public uint Value { get; set; }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(GxVertexAttribute, GxAttributeType?, GxColorComponentType?
      )> GetEnumerator()
    => this.cachedEnumerable_.GetEnumerator();

  public void Read(IBinaryReader br) {
    var impl = new LinkedList<VertexDescriptor>();
    this.map_.Clear();

    while (true) {
      var vertexDescriptor = br.ReadNew<VertexDescriptor>();
      if (vertexDescriptor.Attribute == GxVertexAttribute.NULL) {
        break;
      }

      impl.AddLast(vertexDescriptor);
      this.map_[vertexDescriptor.Attribute] = vertexDescriptor;
    }

    this.cachedEnumerable_
        = impl.Select(v => (
                          v.Attribute,
                          (GxAttributeType?) v.AttributeType,
                          (GxColorComponentType?) v.ColorComponentType)
              );
  }

  public VertexDescriptor? this[GxVertexAttribute attribute] {
    get {
      this.map_.TryGetValue(attribute, out var vertexDescriptor);
      return vertexDescriptor;
    }
  }
}