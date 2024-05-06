using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.tex {
  [BinarySchema]
  public partial class Tex : IBinaryConvertible {
    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public Texture[] textures { get; private set; }
  }
}
