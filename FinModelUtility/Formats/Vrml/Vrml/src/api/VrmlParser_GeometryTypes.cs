using schema.text.reader;

using vrml.schema;

namespace vrml.api;

public partial class VrmlParser {
  private static IndexedFaceSetNode ReadIndexedFaceSetNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IColorNode? color = null;
    bool? colorPerVertex = null;
    bool? convex = null;
    ICoordinateNode coord = default;
    IReadOnlyList<int> coordIndex = default;
    ITextureCoordinateNode? texCoord = null;
    IReadOnlyList<int>? texCoordIndex = null;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "color": {
              color = ParseNodeOfType_<IColorNode>(tr, definitions);
              break;
            }
            case "colorPerVertex": {
              colorPerVertex = ReadBool_(tr);
              break;
            }
            case "convex": {
              convex = ReadBool_(tr);
              break;
            }
            case "coord": {
              coord = ParseNodeOfType_<ICoordinateNode>(tr, definitions);
              break;
            }
            case "coordIndex": {
              coordIndex = ReadIndexArray_(tr);
              break;
            }
            case "texCoord": {
              texCoord
                  = ParseNodeOfType_<ITextureCoordinateNode>(tr, definitions);
              break;
            }
            case "texCoordIndex": {
              texCoordIndex = ReadIndexArray_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new IndexedFaceSetNode {
        Color = color,
        Convex = convex,
        ColorPerVertex = colorPerVertex,
        Coord = coord,
        CoordIndex = coordIndex,
        TexCoord = texCoord,
        TexCoordIndex = texCoordIndex
    };
  }

  private static TextNode ReadTextNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    IReadOnlyList<string> @string = null!;
    IEnumerable<float> length = default;
    IFontStyleNode fontStyle = null!;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "fontStyle": {
              fontStyle = ParseNodeOfType_<IFontStyleNode>(tr, definitions);
              break;
            }
            case "length": {
              length = ReadSingleArray_(tr);
              break;
            }
            case "string": {
              @string = ReadStringArray_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new TextNode {
        String = @string,
        FontStyle = fontStyle,
    };
  }
}