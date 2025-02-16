using System.Numerics;


namespace vrml.schema;

public abstract record BNode : INode {
  public string? DefName { get; set; }
}

public record GroupNode : BNode, IGroupNode {
  public required IReadOnlyList<INode> Children { get; init; }
}

public record TransformNode : BNode, ITransformNode {
  public Vector3? Center { get; init; }
  public required Vector3 Translation { get; init; }
  public Quaternion? Rotation { get; init; }
  public Quaternion? ScaleOrientation { get; init; }
  public Vector3? Scale { get; init; }
  public required IReadOnlyList<INode> Children { get; init; }
}

public record AnchorNode : BNode, IAnchorNode {
  public required string Url { get; init; }
  public required string Description { get; init; }
  public IReadOnlyList<string>? Parameter { get; init; }
  public required IReadOnlyList<INode> Children { get; init; }
}

public record BackgroundNode : BNode, IBackgroundNode {
  public required Vector3 SkyColor { get; init; }
}

public record DirectionalLightNode : BNode, IDirectionalLightNode {
  public required float AmbientIntensity { get; init; }
  public required Vector3 Color { get; init; }
  public required Vector3 Direction { get; init; }
  public required float Intensity { get; init; }
}

public record ShapeNode : BNode, IShapeNode {
  public required AppearanceNode Appearance { get; init; }
  public required IGeometryNode Geometry { get; init; }
}

public record AppearanceNode : BNode {
  public required IMaterialNode Material { get; init; }
  public IImageTextureNode? Texture { get; init; }
  public ITextureTransformNode? TextureTransform { get; init; }
}

public record MaterialNode : BNode, IMaterialNode {
  public const float DEFAULT_AMBIENT_INTENSITY = .2f;
  public const float DEFAULT_DIFFUSE_COLOR = .8f;

  public Vector3? AmbientColor { get; init; }
  public float AmbientIntensity { get; init; } = DEFAULT_AMBIENT_INTENSITY;
  public Vector3 DiffuseColor { get; init; } = new(DEFAULT_DIFFUSE_COLOR);
  public float Transparency { get; init; }
}

public record ImageTextureNode : BNode, IImageTextureNode {
  public required string Url { get; init; }
}

public record TextureTransformNode : BNode, ITextureTransformNode {
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

public record IsbPictureNode : BNode, IIsbPictureNode {
  public Vector3? Center { get; init; }
  public bool? Pinned { get; init; }
  public Quaternion? Rotation { get; init; }
  public Quaternion? ScaleOrientation { get; init; }
  public Vector3? Scale { get; init; }
  public Vector3 Translation { get; init; }
  public required IReadOnlyList<IImageTextureNode> Frames { get; init; }
}

public record ColorNode : BNode, IColorNode {
  public required IReadOnlyList<Vector3> Color { get; init; }
}

public record CoordinateNode : BNode, ICoordinateNode {
  public required IReadOnlyList<Vector3> Point { get; init; }
}

public record TextureCoordinateNode : BNode, ITextureCoordinateNode {
  public required IReadOnlyList<Vector2> Point { get; init; }
}

public record FontStyleNode : BNode, IFontStyleNode {
  public string? Family { get; init; }
  public required IReadOnlyList<string> Justify { get; init; }
  public float? Size { get; init; }
  public required string Style { get; init; }
}

public record RouteNode : BNode {
  public required string Src { get; init; }
  public required string Dst { get; init; }
}