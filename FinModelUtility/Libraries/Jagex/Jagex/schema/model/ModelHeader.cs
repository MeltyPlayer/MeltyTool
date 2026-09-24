using schema.binary;
using schema.binary.attributes;

namespace jagex.schema.model;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ddemon26/2006Scape/blob/678f456faefe24aa7bbfe977c47518cd0919708e/2006Scape%20Client/src/main/java/Model.java#L35
/// </summary>
[BinarySchema]
public sealed partial class ModelHeader : IBinaryConvertible {
  public ushort VertexCount { get; set; }
  public ushort FaceCount { get; set; }
  public byte TexturedTriangleCount { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasFaceLabels { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasFaceAlphas { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasFaceTextures { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasVertexLabels { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasVertexSkins { get; set; }

  public ushort VertexXLength { get; set; }
  public ushort VertexYLength { get; set; }
  public ushort VertexZLength { get; set; }
  public ushort FaceTypeLength { get; set; }
}