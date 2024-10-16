using gx.vertex;

using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.shp1;

[BinarySchema]
public partial class BatchAttribute : IBinaryDeserializable {
  public GxVertexAttribute Attribute { get; set; }
  public GxAttributeType DataType { get; set; }
}