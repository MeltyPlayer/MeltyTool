using schema.binary;
using schema.binary.attributes;

namespace pikmin1.schema.anm {
  [BinarySchema]
  public partial class Anm : IBinaryConvertible {
    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public DcxWrapper[] Wrappers { get; set; }
  }
}