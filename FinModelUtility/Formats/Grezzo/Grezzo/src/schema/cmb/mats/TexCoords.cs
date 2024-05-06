using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.mats {
  [BinarySchema]
  public partial class TexCoords : IBinaryDeserializable {
    public byte coordinateIndex { get; private set; }
    public byte referenceCameraIndex { get; private set; }

    [IntegerFormat(SchemaIntegerType.BYTE)]
    public TextureMappingType mappingMethod { get; private set; }

    [IntegerFormat(SchemaIntegerType.BYTE)]
    public TextureMatrixMode matrixMode { get; private set; }

    public Vector2f scale { get; } = new();
    public Vector2f translation { get; } = new();
    public float rotation { get; private set; }
  }
}