using schema.binary;
using schema.binary.attributes;

namespace pikmin1.schema.mod;

[BinarySchema]
public sealed partial class VtxMatrix : IBinaryConvertible {
  public short index = 0;
}