using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloMesh : IBinaryConvertible {
  [StringLengthSource(24)]
  public string Name { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public GloXyzKey[] MoveKeys { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public GloScaleKey[] ScaleKeys { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public GloQuaternionKey[] RotateKeys { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public Vector3f[] Vertices { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public GloFace[] Faces { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public GloSprite[] Sprites { get; set; }

  [NumberFormat(SchemaNumberType.UN8)]
  public float MeshTranslucency { get; set; }

  private readonly byte padding_ = 0;

  public GloObjectFlags MeshFlags { get; set; }

  public GloMeshPointers Pointers { get; } = new();
}