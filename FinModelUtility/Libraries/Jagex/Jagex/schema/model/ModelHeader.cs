using schema.binary;
using schema.binary.attributes;

namespace jagex.schema.model;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ddemon26/2006Scape/blob/678f456faefe24aa7bbfe977c47518cd0919708e/2006Scape%20Client/src/main/java/Model.java#L35
/// </summary>
[BinarySchema]
public sealed partial class ModelHeader : IBinaryConvertible {
  public ushort VertexCount { get; private set; }
  public ushort FaceCount { get; private set; }
  public byte TexturedTriangleCount { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasFaceLabels { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasFaceAlphas { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasFaceTextures { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasVertexLabels { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasVertexSkins { get; private set; }

  public ushort VertexXLength { get; private set; }
  public ushort VertexYLength { get; private set; }
  public ushort VertexZLength { get; private set; }
  public ushort FaceTypeLength { get; private set; }
}