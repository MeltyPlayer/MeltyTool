using System;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;

using fin.util.asserts;
using fin.util.strings;

using schema.text.reader;

using vrml.schema;

namespace vrml.api;

public class VrmlParser {
  public static IGroupNode Parse(Stream stream) {
    stream = RemoveComments_(stream);
    var tr = new SchemaTextReader(stream);
    var definitions = new Dictionary<string, INode>();
    return new GroupNode {
        Children = ReadChildren_(tr, definitions, false).ToArray()
    };
  }

  private static Stream RemoveComments_(Stream input) {
    using var sr = new StreamReader(input);

    var sb = new StringBuilder();
    while (!sr.EndOfStream) {
      ReadOnlySpan<char> line = sr.ReadLine();
      line = line.SubstringUpTo('#').Trim();

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
      = new[] { "PROTO", "Sound", "World" }.ToImmutableHashSet();

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
      if (tr.Matches(out _, [']'])) {
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
      do {
        var c = tr.ReadChar();
        if (c is '{') {
          level++;
        } else if (c is '}') {
          level--;
        }
      } while (level > 0 && !tr.Eof);

      node = default;
      return false;
    }

    node = nodeType switch {
        "Appearance"   => ReadAppearanceNode_(tr, definitions),
        "Group"        => ReadGroupNode_(tr, definitions),
        "ImageTexture" => ReadImageTextureNode_(tr),
        "Material"     => ReadMaterialNode_(tr, definitions),
        "Shape"        => ReadShapeNode_(tr, definitions),
        "Transform"    => ReadTransformNode_(tr, definitions),
    };

    if (definitionName != null) {
      definitions.Add(definitionName, node);
    }

    return true;
  }

  private static IAppearanceNode ReadAppearanceNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IMaterialNode materialNode = default;
    IImageTextureNode textureNode = default;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "material": {
              materialNode
                  = ParseNodeOfType_<IMaterialNode>(tr, definitions);
              break;
            }
            case "texture": {
              textureNode
                  = ParseNodeOfType_<IImageTextureNode>(tr, definitions);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new AppearanceNode {
        Material = materialNode,
        Texture = textureNode
    };
  }

  private static IGroupNode ReadGroupNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IReadOnlyList<INode> children = default;

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
              url = ReadWord_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new ImageTextureNode { Url = url };
  }

  private static IMaterialNode ReadMaterialNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    float? ambientIntensity = null;
    Vector3 diffuseColor = default;
    float? transparency = null;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
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
        AmbientIntensity = ambientIntensity,
        DiffuseColor = diffuseColor,
        Transparency = transparency
    };
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

  private static ITransformNode ReadTransformNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IReadOnlyList<INode> children = default;
    Quaternion? rotation = null;
    Vector3? scale = null;
    Quaternion? scaleOrientation = null;
    Vector3 translation = default;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "children": {
              children = ReadChildren_(tr, definitions);
              break;
            }
            case "rotation": {
              var floats = ReadSingles_(tr, 4);
              rotation
                  = new Quaternion(floats[0], floats[1], floats[2], floats[3]);
              break;
            }
            case "scale": {
              scale = ReadVector3_(tr);
              break;
            }
            case "scaleOrientation": {
              var floats = ReadSingles_(tr, 4);
              scaleOrientation
                  = new Quaternion(floats[0], floats[1], floats[2], floats[3]);
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
      if (tr.Matches(out _, ['}'])) {
        return;
      }

      var fieldName = ReadWord_(tr);
      fieldHandler(fieldName);
    }
  }

  private static Vector3 ReadVector3_(ITextReader tr)
    => new Vector3(ReadSingles_(tr, 3));


  private static float[] ReadSingles_(ITextReader tr, int count) {
    var singles = tr.ReadSingles(TextReaderConstants.WHITESPACE_STRINGS,
                                 TextReaderConstants.NEWLINE_STRINGS);
    Asserts.Equal(count, singles.Length);
    return singles;
  }

  private static void SkipWhitespace_(ITextReader tr)
    => tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_STRINGS);

  private static string ReadWord_(ITextReader tr) {
    SkipWhitespace_(tr);
    var word
        = tr.ReadUpToStartOfTerminator(TextReaderConstants.WHITESPACE_STRINGS);
    return word;
  }
}