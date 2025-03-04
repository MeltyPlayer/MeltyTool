using gx;
using gx.displayList;

using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema.mesh;

[BinarySchema]
public partial class VertexDescriptor : IBinaryConvertible {
  public GxVertexAttribute Attribute { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT32)]
  public GxAttributeType AttributeType { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT32)]
  public GxComponentCountType ComponentCountType { get; set; }

  [Skip]
  public int ComponentCount
    => GxAttributeUtil.GetComponentCount(this.Attribute,
                                         this.ComponentCountType);

  public GxComponentType RawComponentType { get; set; }

  [Skip]
  public GxAxisComponentType AxesComponentType
    => (GxAxisComponentType) this.RawComponentType;

  [Skip]
  public GxColorComponentType ColorComponentType
    => (GxColorComponentType) this.RawComponentType;


  public byte Scale { get; set; }
  public byte Padding { get; set; }
  public ushort Stride { get; set; }
  public uint ArrayOffset { get; set; }
}