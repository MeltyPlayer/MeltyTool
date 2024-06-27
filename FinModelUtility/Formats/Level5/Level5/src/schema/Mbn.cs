using fin.schema;
using fin.schema.vector;

using schema.binary;

namespace level5.schema;

[BinarySchema]
public partial class Mbn : IBinaryDeserializable {
  public uint Id { get; private set; }

  public uint ParentId { get; private set; }

  [Unknown]
  public int Unknown0 { get; private set; }

  public Vector3f Position { get; } = new();

  public float[] RotationMatrix3 { get; } = new float[9];

  public Vector3f Scale { get; } = new();

  public override string ToString() => $"{Id}";
}