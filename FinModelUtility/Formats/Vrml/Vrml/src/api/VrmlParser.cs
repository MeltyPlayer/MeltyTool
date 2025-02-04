using System.Collections.Immutable;
using System.IO.Compression;
using System.Numerics;
using System.Text;

using fin.schema;
using fin.util.asserts;
using fin.util.strings;

using schema.binary;
using schema.text.reader;

using vrml.schema;


namespace vrml.api;

public partial class VrmlParser {
  public static IGroupNode Parse(Stream stream) {
    DecompressStreamIfNeeded_(ref stream);
    stream = RemoveComments_(stream);
    var tr = new SchemaTextReader(stream);
    var definitions = new Dictionary<string, INode>();
    return new GroupNode {
        Children = ReadChildren_(tr, definitions, false).ToArray()
    };
  }

  private static void DecompressStreamIfNeeded_(ref Stream stream) {
    var baseOffset = stream.Position;

    Span<uint> buffer = stackalloc uint[1];
    var br = new SchemaBinaryReader(stream);
    br.ReadUInt32s(buffer);
    stream.Position = baseOffset;

    if (buffer[0] == 0x88B1F) {
      stream = new GZipStream(stream, CompressionMode.Decompress);
    }
  }

  private static Stream RemoveComments_(Stream input) {
    using var sr = new StreamReader(input);

    var sb = new StringBuilder();
    while (!sr.EndOfStream) {
      ReadOnlySpan<char> line = sr.ReadLine();
      line = line.SubstringUpTo('#').Trim();
      if (line.IsEmpty) {
        continue;
      }

      sb.Append(line);
      sb.AppendLine();
    }

    var output = new MemoryStream();
    var sw = new StreamWriter(output);
    sw.Write(sb);
    sw.Flush();
    output.Position = 0;

    return output;
  }

  private static readonly IImmutableSet<string> UNSUPPORTED_NODES
      = new[] {
              "BackgroundColor", "Collision", "Fog", "Info", "NavigationInfo",
              "OrientationInterpolator", "PerspectiveCamera",
              "PositionInterpolator", "PROTO", "ProximitySensor", "ROUTE",
              "Sphere", "Sound", "TimeSensor", "Viewpoint", "WorldInfo",
              "WWWAnchor",
          }
          .ToImmutableHashSet();

  private static IReadOnlyList<INode> ReadChildren_(
      ITextReader tr,
      IDictionary<string, INode> definitions,
      bool useBrackets = true) {
    if (useBrackets) {
      SkipWhitespace_(tr);
      tr.AssertChar('[');
    }

    var nodes = new LinkedList<INode>();
    while (!tr.Eof) {
      SkipWhitespace_(tr);

      if (tr.Eof) {
        break;
      }

      if (tr.Matches(']')) {
        break;
      }

      if (tr.Eof) {
        break;
      }

      if (TryParseNode_(tr, definitions, out var node)) {
        if (node != null) {
          nodes.AddLast(node);
        }
      } else {
        break;
      }
    }

    return nodes.ToArray();
  }

  public static TNode ParseNodeOfType_<TNode>(
      ITextReader tr,
      IDictionary<string, INode> definitions)
      where TNode : INode {
    Asserts.True(TryParseNode_(tr, definitions, out var node));
    return Asserts.AsA<TNode>(node);
  }

  private static bool TryParseNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions,
      out INode? node) {
    var nodeType = ReadWord_(tr);

    if (nodeType is "USE") {
      var usedName = ReadWord_(tr);
      node = definitions[usedName];
      return true;
    }

    string? definitionName = null;
    if (nodeType is "DEF") {
      definitionName = ReadWord_(tr);
      nodeType = ReadWord_(tr);
    }

    if (UNSUPPORTED_NODES.Contains(nodeType)) {
      tr.ReadUpToAndPastTerminator(["{"]);
      var level = 1;
      while (level > 0 && !tr.Eof) {
        var c = tr.ReadChar();
        if (c is '{') {
          level++;
        } else if (c is '}') {
          level--;
        }
      }

      node = default;
      return true;
    }

    node = nodeType switch {
        "Anchor" => ReadAnchorNode_(tr, definitions),
        "Appearance" => ReadAppearanceNode_(tr, definitions),
        "AudioClip" => ReadAudioClipNode_(tr),
        "Background" => ReadBackgroundNode_(tr),
        "Color" => ReadColorNode_(tr),
        "Coordinate" or "Coordinate3" => ReadCoordinateNode_(tr),
        "DirectionalLight" => ReadDirectionalLightNode_(tr),
        "FontStyle" => ReadFontStyleNode_(tr),
        "Group" => ReadGroupNode_(tr, definitions),
        "ImageTexture" => ReadImageTextureNode_(tr),
        "IndexedFaceSet" => ReadIndexedFaceSetNode_(tr, definitions),
        "ISBMovingTextureTransform" => ReadIsbMovingTextureTransformNode_(tr),
        "ISBPicture" => ReadIsbPictureNode_(tr, definitions),
        "Material" => ReadMaterialNode_(tr),
        "Separator" => ReadSeparatorNode_(tr, definitions),
        "Shape" => ReadShapeNode_(tr, definitions),
        "ShapeHints" => ReadShapeHintsNode_(tr),
        "Sound" => ReadSoundNode_(tr, definitions),
        "Text" => ReadTextNode_(tr, definitions),
        "TextureCoordinate" => ReadTextureCoordinateNode_(tr),
        "TextureTransform" => ReadTextureTransformNode_(tr),
        "Transform"
            or "ISBLandscape" => ReadTransformNode_(tr, definitions),
        _ => throw new NotImplementedException(),
    };

    if (definitionName != null) {
      definitions.Add(definitionName, node);
    }

    return true;
  }

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

  private static IAppearanceNode ReadAppearanceNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IMaterialNode material = default;
    IImageTextureNode texture = default;
    ITextureTransformNode? textureTransform = null;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "material": {
              material = ParseNodeOfType_<IMaterialNode>(tr, definitions);
              break;
            }
            case "texture": {
              texture = ParseNodeOfType_<IImageTextureNode>(tr, definitions);
              break;
            }
            case "textureTransform": {
              textureTransform
                  = ParseNodeOfType_<ITextureTransformNode>(tr, definitions);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new AppearanceNode {
        Material = material,
        Texture = texture,
        TextureTransform = textureTransform,
    };
  }

  private static IBackgroundNode ReadBackgroundNode_(ITextReader tr) {
    Vector3 skyColor = default;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "skyColor": {
              var color = ReadColorArray_(tr);
              Asserts.Equal(1, color.Count);
              skyColor = color[0];
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new BackgroundNode {
        SkyColor = skyColor,
    };
  }

  private static IColorNode ReadColorNode_(ITextReader tr) {
    IReadOnlyList<Vector3> color = default;
    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "color": {
              color = ReadColorArray_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });
    return new ColorNode { Color = color };
  }

  private static ICoordinateNode ReadCoordinateNode_(ITextReader tr) {
    IReadOnlyList<Vector3> point = default;
    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "point": {
              point = ReadVector3Array_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });
    return new CoordinateNode { Point = point };
  }

  private static IDirectionalLightNode
      ReadDirectionalLightNode_(ITextReader tr) {
    float ambientIntensity = 0;
    Vector3 color = Vector3.One;
    Vector3 direction = -Vector3.UnitZ;
    float intensity = 1;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "ambientIntensity": {
              ambientIntensity = tr.ReadSingle();
              break;
            }
            case "color": {
              color = ReadVector3_(tr);
              break;
            }
            case "direction": {
              direction = ReadVector3_(tr);
              break;
            }
            case "intensity": {
              intensity = tr.ReadSingle();
              break;
            }
            case "on": {
              ReadBool_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new DirectionalLightNode {
        AmbientIntensity = ambientIntensity,
        Color = color,
        Direction = direction,
        Intensity = intensity,
    };
  }

  private static IFontStyleNode ReadFontStyleNode_(ITextReader tr) {
    string? family = null;
    IReadOnlyList<string> justify = default;
    float? size = null;
    string style = default;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "family": {
              family = ReadString_(tr);
              break;
            }
            case "justify": {
              justify = ReadStringArray_(tr);
              break;
            }
            case "size": {
              size = tr.ReadSingle();
              break;
            }
            case "style": {
              style = ReadString_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });
    return new FontStyleNode {
        Family = family,
        Justify = justify,
        Size = size,
        Style = style,
    };
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

  private static IImageTextureNode ReadImageTextureNode_(
      ITextReader tr) {
    string url = default;
    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "url": {
              url = ReadString_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });
    return new ImageTextureNode { Url = url };
  }

  private static IIsbMovingTextureTransformNode
      ReadIsbMovingTextureTransformNode_(ITextReader tr) {
    Vector2 translationStep = default;
    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "translationStep": {
              translationStep = ReadVector2_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });
    return new IsbMovingTextureTransformNode {
        TranslationStep = translationStep
    };
  }

  private static IIsbPictureNode ReadIsbPictureNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    Vector3? center = null;
    IReadOnlyList<IImageTextureNode> frames = default;
    bool? pinned = null;
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
            case "frameCount": {
              var frameCount = ReadWord_(tr);
              break;
            }
            case "frames": {
              frames = ParseNodeArrayOfType_<IImageTextureNode>(
                  tr,
                  definitions);
              break;
            }
            case "pinned": {
              pinned = ReadBool_(tr);
              break;
            }
            case "playOrder": {
              var playOrder = ReadWord_(tr);
              break;
            }
            case "rotation": {
              rotation = ReadQuaternion_(tr);
              break;
            }
            case "scale": {
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

    return new IsbPictureNode {
        Center = center,
        Frames = frames,
        Rotation = rotation,
        Scale = scale,
        ScaleOrientation = scaleOrientation,
        Translation = translation
    };
  }

  private static IMaterialNode ReadMaterialNode_(ITextReader tr) {
    Vector3? ambientColor = null;
    float ambientIntensity = .2f;
    Vector3 diffuseColor = new(.8f);
    float transparency = 0;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "ambientColor": {
              ambientColor = ReadVector3_(tr);
              break;
            }
            case "ambientIntensity": {
              ambientIntensity = tr.ReadSingle();
              break;
            }
            case "diffuseColor": {
              diffuseColor = ReadVector3_(tr);
              break;
            }
            case "transparency": {
              transparency = tr.ReadSingle();
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new MaterialNode {
        AmbientColor = ambientColor,
        AmbientIntensity = ambientIntensity,
        DiffuseColor = diffuseColor,
        Transparency = transparency
    };
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

    ICoordinateNode? currentCoord = default;
    IAppearanceNode? currentAppearance = default;
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

  private static IShapeNode ReadShapeNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IAppearanceNode appearanceNode = default;
    IGeometryNode geometryNode = default;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "appearance": {
              appearanceNode
                  = ParseNodeOfType_<IAppearanceNode>(tr, definitions);
              break;
            }
            case "geometry": {
              geometryNode = ParseNodeOfType_<IGeometryNode>(tr, definitions);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new ShapeNode {
        Appearance = appearanceNode,
        Geometry = geometryNode
    };
  }

  private static ITextureCoordinateNode ReadTextureCoordinateNode_(
      ITextReader tr) {
    IReadOnlyList<Vector2> point = default;
    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "point": {
              point = ReadVector2Array_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });
    return new TextureCoordinateNode { Point = point };
  }

  private static ITextureTransformNode ReadTextureTransformNode_(
      ITextReader tr) {
    Vector2? center = null;
    float? rotation = null;
    Vector2? scale = null;
    Vector2? translation = null;
    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "center": {
              center = ReadVector2_(tr);
              break;
            }
            case "rotation": {
              rotation = tr.ReadSingle();
              break;
            }
            case "scale": {
              scale = ReadVector2_(tr);
              break;
            }
            case "translation": {
              translation = ReadVector2_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });
    return new TextureTransformNode {
        Center = center,
        Rotation = rotation,
        Scale = scale,
        Translation = translation
    };
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

  private static void ReadFields_(ITextReader tr, Action<string> fieldHandler) {
    SkipWhitespace_(tr);
    tr.AssertChar('{');

    while (!tr.Eof) {
      SkipWhitespace_(tr);
      if (tr.Matches('}')) {
        return;
      }

      if (tr.Matches(out _, ["ROUTE"])) {
        tr.ReadLine();
        continue;
      }

      var fieldName = ReadWord_(tr);
      fieldHandler(fieldName);
    }
  }

  private static void ReadArray_(ITextReader tr, Action lineHandler) {
    SkipWhitespace_(tr);
    tr.AssertChar('[');

    while (!tr.Eof) {
      SkipWhitespace_(tr);
      if (tr.Matches(out _, [']'])) {
        return;
      }

      lineHandler();
    }
  }

  private static IReadOnlyList<TNode> ParseNodeArrayOfType_<TNode>(
      ITextReader tr,
      IDictionary<string, INode> definitions)
      where TNode : INode {
    var nodes = new LinkedList<TNode>();
    ReadArray_(
        tr,
        () => nodes.AddLast(ParseNodeOfType_<TNode>(tr, definitions)));
    return nodes.ToArray();
  }

  private static IReadOnlyList<int> ReadIndexArray_(ITextReader tr) {
    SkipWhitespace_(tr);
    tr.AssertChar('[');
    return tr.ReadInt32s(TextReaderConstants.COMMA_CHAR, ']');
  }

  private static IReadOnlyList<Vector2> ReadVector2Array_(ITextReader tr) {
    var list = new LinkedList<Vector2>();
    ReadArray_(tr,
               () => {
                 tr.Matches(out _, [',']);
                 list.AddLast(
                     new Vector2(ReadSingles_(tr, 2, [",", "\n", "\r\n"])));
               });
    return list.ToArray();
  }

  private static IReadOnlyList<Vector3> ReadColorArray_(ITextReader tr) {
    var list = new LinkedList<Vector3>();
    ReadArray_(tr, () => list.AddLast(ReadVector3_(tr)));
    return list.ToArray();
  }

  private static IReadOnlyList<Vector3> ReadVector3Array_(ITextReader tr) {
    var list = new LinkedList<Vector3>();
    ReadArray_(tr,
               () => {
                 tr.Matches(out _, [',']);
                 list.AddLast(
                     new Vector3(ReadSingles_(tr, 3, [",", "\n", "\r\n"])));
               });
    return list.ToArray();
  }

  private static IReadOnlyList<string> ReadStringArray_(ITextReader tr) {
    var list = new LinkedList<string>();
    ReadArray_(tr,
               () => {
                 tr.Matches(out _, [',']);
                 list.AddLast(ReadString_(tr));
               });
    return list.ToArray();
  }

  private static bool ReadBool_(ITextReader tr) => ReadWord_(tr) == "TRUE";

  private static Vector2 ReadVector2_(ITextReader tr)
    => new(ReadSingles_(tr, 2));

  private static Vector3 ReadVector3_(ITextReader tr)
    => new(ReadSingles_(tr, 3));

  private static Quaternion ReadQuaternion_(ITextReader tr) {
    var values = ReadSingles_(tr, 4);
    return Quaternion.CreateFromAxisAngle(new Vector3(values.AsSpan(0, 3)),
                                          values[3]);
  }

  private static float[] ReadSingles_(ITextReader tr, int count) {
    var singles = new float[count];
    for (var i = 0; i < count; ++i) {
      singles[i] = float.Parse(ReadWord_(tr));
    }

    return singles;
  }

  private static float[] ReadSingles_(ITextReader tr,
                                      int count,
                                      ReadOnlySpan<string> terminators) {
    var singles = tr.ReadSingles(TextReaderConstantsExtra.WHITESPACE_STRINGS,
                                 terminators);
    Asserts.Equal(count, singles.Length);
    return singles;
  }


  private static void SkipWhitespace_(ITextReader tr)
    => tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

  private static string ReadString_(ITextReader tr) {
    SkipWhitespace_(tr);
    tr.AssertChar('"');
    return tr.ReadUpToAndPastTerminator('"');
  }

  private static string ReadWord_(ITextReader tr) {
    SkipWhitespace_(tr);
    var word
        = tr.ReadUpToStartOfTerminator([" ", "\t", "\n", "\r\n", "{", "["]);
    return word;
  }
}