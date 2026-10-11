using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.polygon_studio;

public enum ShadeType {
  FLAT = 0,
  SMOOTH = 1,
}

[BinarySchema]
public sealed partial class Mesh : IBinaryDeserializable {
  private uint VertexCount { get; set; }
  public uint FaceCount { get; set; }
  private uint TriangleCount { get; set; }
  public ShadeType ShadeType { get; set; }
  public int VertexDefinitionsOffset { get; set; }
  public uint VertexDefinitionsSize { get; set; }
  public int Unk2sOffset { get; set; }
  public uint Unk2sSize { get; set; }
  public int TriangleDefinitionsOffset { get; set; }
  public uint TriangleDefinitionsSize { get; set; }
  public int TextureOffset { get; set; }
  public uint TextureSize { get; set; }
  public uint NextMeshOffset { get; set; }
  public uint EdgeCount { get; set; }

  [RAtPositionOrNull(nameof(VertexDefinitionsOffset), -1)]
  [RSequenceLengthSource(nameof(VertexCount))]
  public Vertex[]? Vertices { get; set; }

  [RAtPositionOrNull(nameof(TriangleDefinitionsOffset), -1)]
  [RSequenceLengthSource(nameof(TriangleCount))]
  public Triangle[]? Triangles { get; set; }

  [Skip]
  public Argb1555Image Texture { get; set; }
}