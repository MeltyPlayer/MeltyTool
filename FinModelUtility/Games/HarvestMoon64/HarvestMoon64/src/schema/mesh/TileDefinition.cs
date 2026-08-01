using schema.binary;
using schema.binary.attributes;

namespace hm64.schema.mesh;

[BinarySchema]
public partial class TileDefinition : IBinaryDeserializable {
  private uint pointer_;

  public byte YOffset { get; set; }
  public byte FallbackH { get; set; }
  public byte RawTexIndex { get; set; }

  [SequenceLengthSource(SchemaIntegerType.BYTE)]
  public Vertex[] Vertices { get; set; }
}