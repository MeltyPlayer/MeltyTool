using System.Numerics;

using fin.util.asserts;

using schema.text.reader;

using vrml.schema;

namespace vrml.api;

public partial class VrmlParser {
  private static IAnchorNode ReadAnchorNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IReadOnlyList<INode> children = [];
    string description = default;
    IReadOnlyList<string> parameter = default;
    string url = default;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "children": {
              children = ReadChildren_(tr, definitions);
              break;
            }
            case "description": {
              description = ReadString_(tr);
              break;
            }
            case "parameter": {
              parameter = ReadStringArray_(tr);
              break;
            }
            case "url": {
              url = ReadString_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new AnchorNode {
        Children = children,
        Description = description,
        Parameter = parameter,
        Url = url,
    };
  }

  private static CollisionNode ReadCollisionNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IReadOnlyList<INode> children = [];
    bool collide;
    ShapeNode proxy;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "children": {
              children = ReadChildren_(tr, definitions);
              break;
            }
            case "collide": {
              collide = ReadBool_(tr);
              break;
            }
            case "proxy": {
              proxy = ParseNodeOfType_<ShapeNode>(tr, definitions);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new CollisionNode(children);
  }

  private static IGroupNode ReadGroupNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IReadOnlyList<INode> children = [];

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "children": {
              children = ReadChildren_(tr, definitions);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new GroupNode { Children = children };
  }

  private static INode ReadSeparatorNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    LinkedList<INode> rawChildren = new();

    SkipWhitespace_(tr);
    tr.AssertChar('{');

    while (!tr.Eof) {
      SkipWhitespace_(tr);
      if (tr.Matches('}')) {
        break;
      }

      if (tr.Eof) {
        break;
      }

      if (TryParseNode_(tr, definitions, out var node)) {
        if (node != null) {
          rawChildren.AddLast(node);
        }
      } else {
        break;
      }
    }

    LinkedList<INode> children = new();

    ICoordinateNode? currentCoord = null;
    AppearanceNode? currentAppearance = null;
    foreach (var child in rawChildren) {
      switch (child) {
        case ICoordinateNode coordNode: {
          currentCoord = coordNode;
          break;
        }
        case IMaterialNode materialNode: {
          currentAppearance = new AppearanceNode { Material = materialNode };
          break;
        }
        case IndexedFaceSetNode indexedFaceSetNode: {
          children.AddLast(new ShapeNode {
              Appearance = Asserts.CastNonnull(currentAppearance),
              Geometry = indexedFaceSetNode with {
                  Coord = currentCoord.AssertNonnull()
              }
          });
          break;
        }
        default: {
          children.AddLast(child);
          break;
        }
      }
    }

    return new GroupNode { Children = children.ToArray() };
  }

  private static ITransformNode ReadTransformNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    Vector3? center = null;
    IReadOnlyList<INode> children = [];
    Quaternion? rotation = null;
    Vector3? scale = null;
    Quaternion? scaleOrientation = null;
    Vector3 translation = default;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "center": {
              center = ReadVector3_(tr);
              break;
            }
            case "children": {
              children = ReadChildren_(tr, definitions);
              break;
            }
            case "rotation": {
              rotation = ReadQuaternion_(tr);
              break;
            }
            case "scale":
            case "scaleFactor": {
              scale = ReadVector3_(tr);
              break;
            }
            case "scaleOrientation": {
              scaleOrientation = ReadQuaternion_(tr);
              break;
            }
            case "translation": {
              translation = ReadVector3_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new TransformNode {
        Center = center,
        Children = children,
        Rotation = rotation,
        Scale = scale,
        ScaleOrientation = scaleOrientation,
        Translation = translation
    };
  }
}