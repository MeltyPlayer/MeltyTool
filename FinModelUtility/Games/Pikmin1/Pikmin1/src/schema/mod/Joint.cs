using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace pikmin1.schema.mod {
  [BinarySchema]
  public partial class JointMatPoly : IBinaryConvertible {
    public ushort matIdx = 0;
    public ushort meshIdx = 0;
  }

  [BinarySchema]
  public partial class Joint : IBinaryConvertible {
    public uint parentIdx = 0;
    public uint flags = 0;
    public Vector3 boundsMax { get; set; }
    public Vector3 boundsMin { get; set; }
    public float volumeRadius = 0;
    public Vector3 scale { get; set; }
    public Vector3 rotation { get; set; }
    public Vector3 position { get; set; }

    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public JointMatPoly[] matpolys;
  }
}