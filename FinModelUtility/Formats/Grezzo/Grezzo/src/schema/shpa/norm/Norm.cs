using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.shpa.norm {

  [BinarySchema]
  public partial class Norm : IBinaryConvertible {
    [RSequenceUntilEndOfStream]
    public ShpaNormal[] Values { get; private set; }
  }
}
