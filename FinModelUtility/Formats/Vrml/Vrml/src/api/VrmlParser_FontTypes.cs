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
              var (major, minor) = ReadJustifies_(tr);
              fontStyle.MajorJustify = major;
              fontStyle.MinorJustify = minor ?? Justify.FIRST;
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

  private static (Justify major, Justify? minor)
      ReadJustifies_(ITextReader tr) {
    var text = ReadStringArray_(tr);
    var major = ReadJustify_(text[0]);
    var minor = text.Count > 1 ? ReadJustify_(text[1]) : (Justify?) null;
    return (major, minor);
  }

  private static Justify ReadJustify_(string text)
    => text switch {
        "BEGIN"  => Justify.BEGIN,
        "FIRST"  => Justify.FIRST,
        "MIDDLE" => Justify.MIDDLE,
        "END"    => Justify.END,
        _        => throw new ArgumentOutOfRangeException()
    };

  private static Style ReadStyle_(ITextReader tr)
    => ReadString_(tr) switch {
        "BOLD"       => Style.BOLD,
        "BOLDITALIC" => Style.BOLD_ITALIC,
        "ITALIC"     => Style.ITALIC,
        "PLAIN"      => Style.PLAIN,
        _            => throw new ArgumentOutOfRangeException()
    };
}