using schema.text.reader;

using vrml.schema;

namespace vrml.api;

public partial class VrmlParser {
  private static FontStyleNode ReadFontStyleNode_(ITextReader tr) {
    string? family = null;
    Justify justify = Justify.BEGIN;
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
              justify = ReadJustify_(tr);
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

  private static Justify ReadJustify_(ITextReader tr)
    => ReadStringArray_(tr).Single() switch {
        "BEGIN"  => Justify.BEGIN,
        "MIDDLE" => Justify.MIDDLE,
        "END"    => Justify.END,
        _        => throw new ArgumentOutOfRangeException()
    };
}