using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.shpa.posi;

[BinarySchema]
public partial class Posi : IBinaryConvertible {
  [RSequenceUntilEndOfStream]
  public Vector3[] Values { get; private set; }
}