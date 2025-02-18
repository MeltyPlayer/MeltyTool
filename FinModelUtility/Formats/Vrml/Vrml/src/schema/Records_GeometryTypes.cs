using System.Numerics;

namespace vrml.schema;

public interface IGeometryNode : INode { }

public record BoxNode : BNode, IGeometryNode {
  public Vector3 Size { get; set; }
}

public record IndexedFaceSetNode : BNode, IGeometryNode {
  public bool? Convex { get; init; }
  public bool? ColorPerVertex { get; init; }
  public IColorNode? Color { get; init; }
  public required ICoordinateNode Coord { get; init; }
  public required IReadOnlyList<int> CoordIndex { get; init; }
  public ITextureCoordinateNode? TexCoord { get; init; }
  public IReadOnlyList<int>? TexCoordIndex { get; init; }
}

public record SphereNode : BNode, IGeometryNode {
  public float Radius { get; set; }
}

public record TextNode : BNode, IGeometryNode {
  public required IReadOnlyList<string> String { get; init; }
  public required FontStyleNode FontStyle { get; init; }
}