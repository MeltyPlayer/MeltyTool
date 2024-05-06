using schema.binary;

namespace grezzo.schema.cmb.vatr {
  [BinarySchema]
  public partial class AttributeSlice : IBinaryConvertible {
    public uint Size { get; private set; }
    public uint StartOffset { get; private set; }
  }
}