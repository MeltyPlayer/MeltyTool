using fin.schema.vector;

using schema.binary;

namespace mod.schema.mod;

[BinarySchema]
public partial class Nbt : IBinaryConvertible {
  public Vector3f Normal { get; }= new();
  public Vector3f Binormal { get; }= new();
  public Vector3f Tangent { get; }= new();
}