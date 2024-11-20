using System.Numerics;

using fin.schema;

using schema.binary;

namespace level5.schema;

[BinarySchema]
public partial class Mbn : IBinaryDeserializable {
  public uint Id { get; private set; }

  public uint ParentId { get; private set; }

  [Unknown]
  public int Unknown0 { get; private set; }

  public Vector3 Position { get; private set; }

  public float[] RotationMatrix3 { get; } = new float[9];

  public Vector3 Scale { get; private set; }

  public override string ToString() => $"{this.Id}";
}