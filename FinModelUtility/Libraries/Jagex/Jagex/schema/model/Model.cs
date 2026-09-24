using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace jagex.schema.model;

[Flags]
public enum VertexFlags : byte {
  HAS_X_OFFSET,
  HAS_Y_OFFSET,
  HAS_Z_OFFSET,
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ddemon26/2006Scape/blob/678f456faefe24aa7bbfe977c47518cd0919708e/2006Scape%20Client/src/main/java/Model.java#L142
/// </summary>
[BinarySchema]
public sealed partial class Model : IBinaryConvertible {
  public ModelHeader Header { get; } = new();

  [RSequenceLengthSource(nameof(Header.VertexCount))]
  public VertexFlags[] VertexFlags { get; set; }

  [RSequenceLengthSource(nameof(Header.FaceCount))]
  public byte[] FacePriorities { get; set; }

  [RIfBoolean(nameof(Header.HasFaceAlphas))]
  [RSequenceLengthSource(nameof(Header.FaceCount))]
  public byte[]? FaceAlphas { get; set; }

  [RIfBoolean(nameof(Header.HasVertexLabels))]
  [RSequenceLengthSource(nameof(Header.FaceCount))]
  public byte[]? VertexLabels { get; set; }

  [RIfBoolean(nameof(Header.HasFaceLabels))]
  [RSequenceLengthSource(nameof(Header.FaceCount))]
  public byte[]? FaceLabels { get; set; }

  [RIfBoolean(nameof(Header.HasVertexSkins))]
  [RSequenceLengthSource(nameof(Header.VertexCount))]
  public byte[]? VertexSkins { get; set; }

  [RIfBoolean(nameof(Header.HasFaceTextures))]
  [RSequenceLengthSource(nameof(Header.FaceCount))]
  public byte[]? FaceTextures { get; set; }

  [RSequenceLengthSource(nameof(Header.FaceTypeCount))]
  public byte[] FaceTypes { get; set; }

  [RSequenceLengthSource(nameof(Header.FaceCount))]
  public ushort[] FaceSkins { get; set; }

  [RSequenceLengthSource(nameof(Header.TexturedTriangleCount))]
  public Vector2s[] FaceIndices { get; set; }

  [RSequenceLengthSource(nameof(Header.VertexXCount))]
  public byte[] VertexXs { get; set; }

  [RSequenceLengthSource(nameof(Header.VertexYCount))]
  public byte[] VertexYs { get; set; }

  [RSequenceLengthSource(nameof(Header.VertexZCount))]
  public byte[] VertexZs { get; set; }

}