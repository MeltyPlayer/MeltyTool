using System.Numerics;

using fin.schema.vector;
using fin.util.enums;

using schema.binary;
using schema.binary.attributes;

namespace jagex.schema.model;

public enum FacePriority : byte {
  TYPE_1 = 1,
  TYPE_2 = 2,
  TYPE_3 = 3,
  TYPE_4 = 4,
}

[Flags]
public enum VertexFlags : byte {
  HAS_X = 1 << 0,
  HAS_Y = 1 << 1,
  HAS_Z = 1 << 2,
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
  public FacePriority[] FacePriorities { get; set; }

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

  [RSequenceLengthSource(nameof(Header.FaceTypeLength))]
  public byte[] FaceTypes { get; set; }

  [RSequenceLengthSource(nameof(Header.FaceCount))]
  public ushort[] FaceSkins { get; set; }

  [RSequenceLengthSource(nameof(Header.TexturedTriangleCount))]
  public Vector2s[] FaceIndices { get; set; }

  [Skip]
  public Vector3[] Vertices { get; set; }

  [ReadLogic]
  private void ReadVertices_(IBinaryReader br) {
    var baseOffset = br.Position;
    var vertexXValues
        = this.ReadVertexAxis_(br, jagex.schema.model.VertexFlags.HAS_X);

    br.Position = baseOffset + this.Header.VertexXLength;
    baseOffset = br.Position;
    var vertexYValues
        = this.ReadVertexAxis_(br, jagex.schema.model.VertexFlags.HAS_Y);

    br.Position = baseOffset + this.Header.VertexYLength;
    var vertexZValues
        = this.ReadVertexAxis_(br, jagex.schema.model.VertexFlags.HAS_Z);

    this.Vertices = new Vector3[this.Header.VertexCount];

    var previousVertex = Vector3.Zero;
    for (var i = 0; i < this.Vertices.Length; ++i) {
      var delta = new Vector3(
          vertexXValues[i],
          vertexYValues[i],
          vertexZValues[i]);

      previousVertex = this.Vertices[i] = previousVertex + delta;
    }
  }

  private short[] ReadVertexAxis_(
      IBinaryReader br,
      VertexFlags check) {
    var values = new short[this.Header.VertexCount];
    for (var i = 0; i < values.Length; ++i) {
      var vertexFlag = this.VertexFlags[i];
      if (!vertexFlag.CheckFlag(check)) {
        continue;
      }

      values[i] = ReadSigned_(br);
    }

    return values;
  }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/Ddemon26/2006Scape/blob/master/2006Scape%20Client/src/main/java/Stream.java#L195
  /// </summary>
  private static short ReadSigned_(IBinaryReader br) {
    var tmpOffset = br.Position;

    var peek = br.ReadByte();
    if (peek < 128) {
      return (short) (peek - 64);
    }

    br.Position = tmpOffset;
    return (short) (br.ReadUInt16() - 49152);
  }
}