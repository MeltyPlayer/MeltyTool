using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.shpa.norm;

[BinarySchema]
public partial class ShpaNormal : IBinaryConvertible {
  [NumberFormat(SchemaNumberType.SN16)]
  public float NrmX { get; set; }

  [NumberFormat(SchemaNumberType.SN16)]
  public float NrmY { get; set; }

  [NumberFormat(SchemaNumberType.SN16)]
  public float NrmZ { get; set; }


  [NumberFormat(SchemaNumberType.SN16)]
  public float TangentX { get; set; }

  [NumberFormat(SchemaNumberType.SN16)]
  public float TangentY { get; set; }

  [NumberFormat(SchemaNumberType.SN16)]
  public float TangentZ { get; set; }

}