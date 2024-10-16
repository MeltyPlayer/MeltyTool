using gx;
using gx.vertex;

using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema.mesh;

public enum ColorComponentType : uint {
  RGB565,
  RGB888,
  RGBX8888,
  RGBA4444,
  RGBA6,
  RGBA8888,
}

[BinarySchema]
public partial class VertexDescriptor : IBinaryConvertible {
  public GxVertexAttribute Attribute { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT32)]
  public GxAttributeType AttributeType { get; set; }


  [IntegerFormat(SchemaIntegerType.UINT32)]
  public GxComponentCount ComponentCountType { get; set; }

  [Skip]
  public int ComponentCount => this.Attribute switch {
      GxVertexAttribute.Position => this.ComponentCountType switch {
          GxComponentCount.POS_XY  => 2,
          GxComponentCount.POS_XYZ => 3,
      },
      GxVertexAttribute.Normal => this.ComponentCountType switch {
          GxComponentCount.NRM_XYZ => 3,
      },
      GxVertexAttribute.NBT => this.ComponentCountType switch {
          GxComponentCount.NRM_NBT => 3,
      },
      GxVertexAttribute.Tex0Coord or GxVertexAttribute.Tex1Coord => this
          .ComponentCountType switch {
          GxComponentCount.TEX_S  => 1,
          GxComponentCount.TEX_ST => 2,
      },
  };


  [IntegerFormat(SchemaIntegerType.UINT32)]
  public uint RawComponentType { get; set; }

  [Skip]
  public GxComponentType AxesComponentType
    => (GxComponentType) this.RawComponentType;

  [Skip]
  public ColorComponentType ColorComponentType
    => (ColorComponentType) this.RawComponentType;


  public byte Scale { get; set; }

  public byte Padding { get; set; }

  public ushort Stride { get; set; }

  public uint ArrayOffset { get; set; }
}