using System.Collections;
using System.Collections.Generic;

using gx.displayList;

using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.shp1;

public partial class BatchAttributes
    : IVertexDescriptor, IBinaryDeserializable {
  private readonly LinkedList<(GxVertexAttribute, GxAttributeType?)> impl_
      = new();

  public uint Value { get; set; }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(GxVertexAttribute, GxAttributeType?)> GetEnumerator()
    => this.impl_.GetEnumerator();

  public void Read(IBinaryReader br) {
    this.impl_.Clear();

    GxVertexAttribute attribute;
    do {
      attribute = (GxVertexAttribute) br.ReadUInt32();
      var dataType = (GxAttributeType) br.ReadUInt32();

      this.impl_.AddLast((attribute, dataType));
    } while ((byte) attribute != byte.MaxValue);

    this.impl_.RemoveLast();
  }
}