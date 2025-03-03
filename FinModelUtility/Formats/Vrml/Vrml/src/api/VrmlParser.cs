using System.Collections.Immutable;
using System.IO.Compression;
using System.Numerics;
using System.Text;

using fin.schema;
using fin.util.asserts;

using schema.binary;
using schema.text.reader;

using vrml.schema;


namespace vrml.api;

public partial class VrmlParser {
  public static (IGroupNode, IReadOnlyDictionary<string, INode>) Parse(
      Stream stream) {
    DecompressStreamIfNeeded_(ref stream);
    stream = RemoveComments_(stream);
    var tr = new SchemaTextReader(stream);
    var definitions = new Dictionary<string, INode>();
    return (new GroupNode {
        Children = ReadChildren_(tr, definitions, false).ToArray()
    }, definitions);
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

      var lineLength = 0;
      var inString = false;
      foreach (var c in line) {
        if (c == '\"') {
          inString = !inString;
        }

        if (!inString && c == '#') {
          line = line[..lineLength];
          break;
        }

        lineLength++;
      }

      line = line.Trim();
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
              "BackgroundColor", "ColorInterpolator", "Fog", "Info",
              "NavigationInfo", "PerspectiveCamera", "PROTO", "ProximitySensor",
              "Script", "Sound", "TouchSensor", "Viewpoint", "VisibilitySensor",
              "WorldInfo", "WWWAnchor",
          }
          .ToImmutableHashSet();

  private static IReadOnlyList<INode> ReadChildren_(
      ITextReader tr,
      IDictionary<string, INode> definitions,
      bool useBrackets = true) {
    if (useBrackets) {
      tr.SkipWhitespace();
      tr.AssertChar('[');
    }

    var nodes = new LinkedList<INode>();
    while (!tr.Eof) {
      tr.SkipWhitespace();

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
    var nodeType = tr.ReadWord();

    if (nodeType is "USE") {
      var usedName = tr.ReadWord();
      node = definitions[usedName];
      return true;
    }

    string? definitionName = null;
    if (nodeType is "DEF") {
      definitionName = tr.ReadWord();
      nodeType = tr.ReadWord();
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
        "Box" => ReadBoxNode_(tr),
        "Collision" => ReadCollisionNode_(tr, definitions),
        "Color" => ReadColorNode_(tr),
        "Coordinate" or "Coordinate3" => ReadCoordinateNode_(tr),
        "Cylinder" => ReadCylinderNode_(tr),
        "DirectionalLight" => ReadDirectionalLightNode_(tr),
        "FontStyle" => ReadFontStyleNode_(tr),
        "Group" => ReadGroupNode_(tr, definitions),
        "ImageTexture" => ReadImageTextureNode_(tr),
        "IndexedFaceSet" => ReadIndexedFaceSetNode_(tr, definitions),
        "ISBMovingTextureTransform" => ReadIsbMovingTextureTransformNode_(tr),
        "ISBPicture" => ReadIsbPictureNode_(tr, definitions),
        "Material" => ReadMaterialNode_(tr),
        "OrientationInterpolator" => ReadOrientationInterpolatorNode_(tr),
        "PositionInterpolator" => ReadPositionInterpolatorNode_(tr),
        "ROUTE" => ReadRouteNode_(tr),
        "Separator" => ReadSeparatorNode_(tr, definitions),
        "Shape" => ReadShapeNode_(tr, definitions),
        "ShapeHints" => ReadShapeHintsNode_(tr),
        "Sound" => ReadSoundNode_(tr, definitions),
        "Sphere" => ReadSphereNode_(tr),
        "Text" => ReadTextNode_(tr, definitions),
        "TextureCoordinate" => ReadTextureCoordinateNode_(tr),
        "TextureTransform" => ReadTextureTransformNode_(tr),
        "TimeSensor" => ReadTimeSensorNode_(tr),
        "Transform"
            or "ISBLandscape" => ReadTransformNode_(tr, definitions),
        _ => throw new NotImplementedException(),
    };

    if (definitionName != null) {
      node.DefName = definitionName;
      definitions.Add(definitionName, node);
    }

    return true;
  }

  private static AppearanceNode ReadAppearanceNode_(
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
              var frameCount = tr.ReadWord();
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
              var playOrder = tr.ReadWord();
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
        Frames = frames.AssertNonnull(),
        Rotation = rotation,
        Scale = scale,
        ScaleOrientation = scaleOrientation,
        Translation = translation
    };
  }

  private static IMaterialNode ReadMaterialNode_(ITextReader tr) {
    Vector3? ambientColor = null;
    float ambientIntensity = MaterialNode.DEFAULT_AMBIENT_INTENSITY;
    Vector3 diffuseColor = new(MaterialNode.DEFAULT_DIFFUSE_COLOR);
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

  private static IShapeNode ReadShapeNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    AppearanceNode appearanceNode = null!;
    IGeometryNode geometryNode = null!;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "appearance": {
              appearanceNode
                  = ParseNodeOfType_<AppearanceNode>(tr, definitions);
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

  private static RouteNode ReadRouteNode_(ITextReader tr) {
    var src = tr.ReadWord();
    tr.SkipWhitespace();
    tr.AssertString("TO");
    var dst = tr.ReadWord();
    return new RouteNode { Src = src, Dst = dst };
  }

  private static void ReadFields_(ITextReader tr, Action<string> fieldHandler) {
    tr.SkipWhitespace();
    tr.AssertChar('{');

    while (!tr.Eof) {
      tr.SkipWhitespace();
      if (tr.Matches('}')) {
        return;
      }

      if (tr.Matches(out _, ["ROUTE"])) {
        tr.ReadLine();
        continue;
      }

      var fieldName = tr.ReadWord();
      fieldHandler(fieldName);
    }
  }

  private static void ReadArray_(ITextReader tr,
                                 Action<ITextReader> lineHandler) {
    tr.SkipWhitespace();
    tr.AssertChar('[');

    var arrayText = tr.ReadUpToAndPastTerminator(']');

    using var ms = new MemoryStream(Encoding.ASCII.GetBytes(arrayText));
    var subTr = new SchemaTextReader(ms);

    while (!subTr.Eof) {
      subTr.SkipWhitespace();
      lineHandler(subTr);
      subTr.SkipWhitespace();
    }
  }

  private static IReadOnlyList<TNode> ParseNodeArrayOfType_<TNode>(
      ITextReader tr,
      IDictionary<string, INode> definitions)
      where TNode : INode {
    var nodes = new LinkedList<TNode>();
    ReadArray_(
        tr,
        subTr => nodes.AddLast(ParseNodeOfType_<TNode>(subTr, definitions)));
    return nodes.ToArray();
  }

  private static IReadOnlyList<int> ReadIndexArray_(ITextReader tr) {
    tr.SkipWhitespace();
    tr.AssertChar('[');
    return tr.ReadInt32s(TextReaderConstants.COMMA_CHAR, ']');
  }

  private static readonly char[] SEPARATORS_ = TextReaderConstants
                                               .WHITESPACE_CHARS
                                               .Concat(
                                                   TextReaderConstants
                                                       .COMMA_CHARS)
                                               .ToArray();

  private static IReadOnlyList<float> ReadSingleArray_(ITextReader tr) {
    tr.SkipWhitespace();
    tr.AssertChar('[');
    return tr.ReadSingles(SEPARATORS_, [']']);
  }

  private static IReadOnlyList<Vector2> ReadVector2Array_(ITextReader tr) {
    var list = new LinkedList<Vector2>();
    ReadArray_(tr,
               subTr => {
                 subTr.Matches(out _, [',']);
                 list.AddLast(new Vector2(ReadSingles_(subTr, 2)));
               });
    return list.ToArray();
  }

  private static IReadOnlyList<Vector3> ReadColorArray_(ITextReader tr) {
    var list = new LinkedList<Vector3>();
    ReadArray_(tr, subTr => list.AddLast(ReadVector3_(subTr)));
    return list.ToArray();
  }

  private static IReadOnlyList<Vector3> ReadVector3Array_(ITextReader tr) {
    var list = new LinkedList<Vector3>();
    ReadArray_(tr,
               subTr => {
                 subTr.Matches(out _, [',']);
                 list.AddLast(new Vector3(ReadSingles_(subTr, 3)));
               });
    return list.ToArray();
  }

  private static IReadOnlyList<Quaternion>
      ReadQuaternionArray_(ITextReader tr) {
    var list = new LinkedList<Quaternion>();
    ReadArray_(tr,
               subTr => {
                 subTr.Matches(out _, [',']);
                 var values = ReadSingles_(subTr, 4);
                 list.AddLast(Quaternion.CreateFromAxisAngle(
                                  new Vector3(values.AsSpan(0, 3)),
                                  values[3]));
               });
    return list.ToArray();
  }

  private static IReadOnlyList<string> ReadStringArray_(ITextReader tr) {
    var list = new LinkedList<string>();
    ReadArray_(tr,
               subTr => {
                 subTr.Matches(out _, [',']);
                 list.AddLast(ReadString_(subTr));
               });
    return list.ToArray();
  }

  private static bool ReadBool_(ITextReader tr) => tr.ReadWord() == "TRUE";

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
      singles[i] = float.Parse(tr.ReadWord());
      if (!tr.Eof) {
        tr.SkipOnceIfPresent(',');
      }
    }

    return singles;
  }

  private static string ReadString_(ITextReader tr) {
    tr.SkipWhitespace();
    tr.AssertChar('"');
    return tr.ReadUpToAndPastTerminator('"');
  }
}