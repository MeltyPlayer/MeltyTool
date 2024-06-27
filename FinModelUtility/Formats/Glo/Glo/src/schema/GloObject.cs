using schema.binary;
using schema.binary.attributes;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloObject : IBinaryConvertible {
  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public GloAnimSeg[] AnimSegs { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public GloMesh[] Meshes { get; set; }
}