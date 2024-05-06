using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.shpa.norm {
  [BinarySchema]
  public partial class ShpaNormal : IBinaryConvertible {
    [NumberFormat(SchemaNumberType.SN16)]
    public float X { get; set; }

    [NumberFormat(SchemaNumberType.SN16)]
    public float Y { get; set; }

    [NumberFormat(SchemaNumberType.SN16)]
    public float Z { get; set; }
  }
}
