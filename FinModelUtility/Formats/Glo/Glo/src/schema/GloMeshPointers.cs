using schema.binary;
using schema.binary.attributes;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloMeshPointers : IBinaryConvertible {
  [IfBoolean(SchemaIntegerType.UINT16)]
  public GloMesh? Child { get; set; }

  [IfBoolean(SchemaIntegerType.UINT16)]
  public GloMesh? Next { get; set; }
}