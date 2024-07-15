using System.Numerics;


namespace vrml.schema;

public interface INode { }

public interface IDefinitionNode : INode {
  string DefinitionName { get; }
  INode Definition { get; }
}

public interface IGroupNode : INode {
  IReadOnlyList<INode> Children { get; }
}

public interface ITransformNode : IGroupNode {
  Vector3 Translation { get; }
  Quaternion? Rotation { get; }
  Quaternion? ScaleOrientation { get; }
  Vector3? Scale { get; }
}

public interface IIsbLandscapeNode : ITransformNode {}

public interface IAnchorNode : IGroupNode {
  string Url { get; }
  string Description { get; }
}

public interface IShapeNode : INode {
  IAppearanceNode Appearance { get; }
  IIndexedFaceSetNode Geometry { get; }
}

public interface IAppearanceNode : INode {
  IMaterialNode Material { get; }
  ITextureNode? Texture { get; }
  ITextureTransformNode? TextureTransform { get; }
}

public interface IMaterialNode : INode {
  Vector3 DiffuseColor { get; }
  float? AmbientIntensity { get; }
  float? Transparency { get; }
}

public interface ITextureNode : INode {
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

public interface IGeometryNode : INode { }

public interface IIndexedFaceSetNode : IGeometryNode {
  bool? Convex { get; }
  bool? ColorPerVertex { get; }
  IColorNode? Color { get; }
  ICoordinateNode Coord { get; }
  IReadOnlyList<int> CoordIndex { get; }
  ITextureCoordinateNode? TexCoord { get; }
  IReadOnlyList<int>? TexCoordIndex { get; }
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

public interface ITextNode : IGeometryNode {
  IReadOnlyList<string> String { get; }
  IFontStyleNode FontStyle { get; }
}

public interface IFontStyleNode : INode {
  string? Family { get; }
  string Style { get; }
  string Justify { get; }
}