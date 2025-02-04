namespace vrml.schema;

public interface IGeometryNode : INode { }

public record IndexedFaceSetNode : IGeometryNode {
  public bool? Convex { get; init; }
  public bool? ColorPerVertex { get; init; }
  public IColorNode? Color { get; init; }
  public required ICoordinateNode Coord { get; init; }
  public required IReadOnlyList<int> CoordIndex { get; init; }
  public ITextureCoordinateNode? TexCoord { get; init; }
  public IReadOnlyList<int>? TexCoordIndex { get; init; }
}

public record TextNode : IGeometryNode {
  public required IReadOnlyList<string> String { get; init; }
  public required IFontStyleNode FontStyle { get; init; }
}