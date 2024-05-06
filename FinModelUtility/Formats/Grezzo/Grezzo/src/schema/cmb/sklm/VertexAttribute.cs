using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.sklm {
  [BinarySchema]
  public partial class VertexAttribute : IBinaryConvertible {
    public uint Start { get; private set; }
    public float Scale { get; private set; } 

    [IntegerFormat(SchemaIntegerType.UINT16)]
    public DataType DataType { get; private set; }
    
    [IntegerFormat(SchemaIntegerType.UINT16)]
    public VertexAttributeMode Mode { get; private set; }

    public float[] Constants { get; } = new float[4];
  }
}