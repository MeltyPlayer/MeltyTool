namespace modl.schema.modl.common;

public class BwMesh {
  public uint Flags { get; set; }
  public uint MaterialIndex { get; set; }
  public List<BwTriangleStrip> TriangleStrips { get; set; }
}

public class BwTriangleStrip {
  public IReadOnlyList<BwVertexAttributeIndices> VertexAttributeIndicesList {
    get;
    set;
  }
}

public struct BwVertexAttributeIndices {
  public ushort PositionIndex { get; set; }
  public ushort? NormalIndex { get; set; }
  public int? NodeIndex { get; set; }
  public required ushort?[] TexCoordIndices { get; init; }
}