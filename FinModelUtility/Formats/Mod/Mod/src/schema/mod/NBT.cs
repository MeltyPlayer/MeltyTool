using System.Numerics;

using schema.binary;

namespace mod.schema.mod {
  [BinarySchema]
  public partial class Nbt : IBinaryConvertible {
    public Vector3 Normal { get; private set; }
    public Vector3 Binormal { get; private set; }
    public Vector3 Tangent { get; private set; }
  }
}