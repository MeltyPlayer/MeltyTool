using schema.text.reader;

using vrml.schema;

namespace vrml.api;

public partial class VrmlParser {
  private static FontStyleNode ReadFontStyleNode_(ITextReader tr) {
    var fontStyle = new FontStyleNode();

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "family": {
              fontStyle.Family = ReadFamily_(tr);
              break;
            }
            case "justify": {
              fontStyle.Justify = ReadJustify_(tr);
              break;
            }
            case "size": {
              fontStyle.Size = tr.ReadSingle();
              break;
            }
            case "style": {
              fontStyle.Style = ReadStyle_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return fontStyle;
  }

  private static Family ReadFamily_(ITextReader tr)
    => ReadString_(tr) switch {
        "SANS"       => Family.SANS,
        "SERIF"      => Family.SERIF,
        "TYPEWRITER" => Family.TYPEWRITER,
        _            => throw new ArgumentOutOfRangeException()
    };

  private static Justify ReadJustify_(ITextReader tr)
    => ReadStringArray_(tr).Single() switch {
        "BEGIN"  => Justify.BEGIN,
        "MIDDLE" => Justify.MIDDLE,
        "END"    => Justify.END,
        _        => throw new ArgumentOutOfRangeException()
    };

  private static Style ReadStyle_(ITextReader tr)
    => ReadString_(tr) switch {
        "BOLD" => Style.BOLD,
        "BOLDITALIC" => Style.BOLD_ITALIC,
        "ITALIC" => Style.ITALIC,
        "PLAIN" => Style.PLAIN,
        _       => throw new ArgumentOutOfRangeException()
    };
}