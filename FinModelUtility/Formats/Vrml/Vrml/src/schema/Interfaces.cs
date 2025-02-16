using System.Numerics;


namespace vrml.schema;

public interface INode {
  string? DefName { get; set; }
}

public interface IGroupNode : INode {
  IReadOnlyList<INode> Children { get; }
}

public interface ITransform : INode {
  Vector3? Center { get; }
  Vector3 Translation { get; }
  Quaternion? Rotation { get; }
  Quaternion? ScaleOrientation { get; }
  Vector3? Scale { get; }
}

public interface ITransformNode : IGroupNode, ITransform;

public interface IAnchorNode : IGroupNode {
  string Url { get; }
  string Description { get; }
  IReadOnlyList<string>? Parameter { get; }
}

public interface IBackgroundNode : INode {
  Vector3 SkyColor { get; }
}

public interface IDirectionalLightNode : INode {
  float AmbientIntensity { get; }
  Vector3 Color { get; }
  Vector3 Direction { get; }
  float Intensity { get; }
}

public interface IShapeNode : INode {
  AppearanceNode Appearance { get; }
  IGeometryNode Geometry { get; }
}

public interface IMaterialNode : INode {
  Vector3? AmbientColor { get; init; }
  float AmbientIntensity { get; }
  Vector3 DiffuseColor { get; }
  float Transparency { get; }
}

public interface IImageTextureNode : INode {
  string Url { get; }
}

public interface ITextureTransformNode : INode {
  Vector2? Center { get; }
  float? Rotation { get; }
  Vector2? Scale { get; }
  Vector2? Translation { get; }
}

public interface IIsbMovingTextureTransformNode : ITextureTransformNode {
  Vector2 TranslationStep { get; }
}

public interface IIsbPictureNode : INode, ITransform {
  IReadOnlyList<IImageTextureNode> Frames { get; }
  bool? Pinned { get; }
}

public interface IColorNode : INode {
  IReadOnlyList<Vector3> Color { get; }
}

public interface ICoordinateNode : INode {
  IReadOnlyList<Vector3> Point { get; }
}

public interface ITextureCoordinateNode : INode {
  IReadOnlyList<Vector2> Point { get; }
}

public interface IFontStyleNode : INode {
  string? Family { get; }
  IReadOnlyList<string> Justify { get; }
  float? Size { get; }
  string Style { get; }
}