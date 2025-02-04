using System.Numerics;


namespace vrml.schema;

public record GroupNode : IGroupNode {
  public required IReadOnlyList<INode> Children { get; init; }
}

public record TransformNode : ITransformNode {
  public Vector3? Center { get; init; }
  public required Vector3 Translation { get; init; }
  public Quaternion? Rotation { get; init; }
  public Quaternion? ScaleOrientation { get; init; }
  public Vector3? Scale { get; init; }
  public required IReadOnlyList<INode> Children { get; init; }
}

public record AnchorNode : IAnchorNode {
  public required string Url { get; init; }
  public required string Description { get; init; }
  public IReadOnlyList<string>? Parameter { get; init; }
  public required IReadOnlyList<INode> Children { get; init; }
}

public record BackgroundNode : IBackgroundNode {
  public required Vector3 SkyColor { get; init; }
}

public record DirectionalLightNode : IDirectionalLightNode {
  public required float AmbientIntensity { get; init; }
  public required Vector3 Color { get; init; }
  public required Vector3 Direction { get; init; }
  public required float Intensity { get; init; }
}

public record ShapeNode : IShapeNode {
  public required IAppearanceNode Appearance { get; init; }
  public required IGeometryNode Geometry { get; init; }
}

public record AppearanceNode : IAppearanceNode {
  public required IMaterialNode Material { get; init; }
  public IImageTextureNode? Texture { get; init; }
  public ITextureTransformNode? TextureTransform { get; init; }
}

public record MaterialNode : IMaterialNode {
  public Vector3? AmbientColor { get; init; }
  public float AmbientIntensity { get; init; }
  public Vector3 DiffuseColor { get; init; }
  public float Transparency { get; init; }
}

public record ImageTextureNode : IImageTextureNode {
  public required string Url { get; init; }
}

public record TextureTransformNode : ITextureTransformNode {
  public Vector2? Center { get; init; }
  public float? Rotation { get; init; }
  public Vector2? Scale { get; init; }
  public Vector2? Translation { get; init; }
}

public record IsbMovingTextureTransformNode
    : TextureTransformNode,
      IIsbMovingTextureTransformNode {
  public required Vector2 TranslationStep { get; init; }
}

public record IsbPictureNode : IIsbPictureNode {
  public Vector3? Center { get; init; }
  public bool? Pinned { get; init; }
  public Quaternion? Rotation { get; init; }
  public Quaternion? ScaleOrientation { get; init; }
  public Vector3? Scale { get; init; }
  public Vector3 Translation { get; init; }
  public IReadOnlyList<IImageTextureNode> Frames { get; init; }
}

public record IndexedFaceSetNode : IIndexedFaceSetNode {
  public bool? Convex { get; init; }
  public bool? ColorPerVertex { get; init; }
  public IColorNode? Color { get; init; }
  public required ICoordinateNode Coord { get; init; }
  public required IReadOnlyList<int> CoordIndex { get; init; }
  public ITextureCoordinateNode? TexCoord { get; init; }
  public IReadOnlyList<int>? TexCoordIndex { get; init; }
}

public record ColorNode : IColorNode {
  public required IReadOnlyList<Vector3> Color { get; init; }
}

public record CoordinateNode : ICoordinateNode {
  public required IReadOnlyList<Vector3> Point { get; init; }
}

public record TextureCoordinateNode : ITextureCoordinateNode {
  public required IReadOnlyList<Vector2> Point { get; init; }
}

public record TextNode : ITextNode {
  public required IReadOnlyList<string> String { get; init; }
  public required IFontStyleNode FontStyle { get; init; }
}

public record FontStyleNode : IFontStyleNode {
  public string? Family { get; init; }
  public required IReadOnlyList<string> Justify { get; init; }
  public float? Size { get; init; }
  public required string Style { get; init; }
}