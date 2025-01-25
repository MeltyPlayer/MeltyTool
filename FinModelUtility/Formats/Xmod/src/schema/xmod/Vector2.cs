using fin.model;
using fin.schema;
using fin.util.asserts;

using schema.text;
using schema.text.reader;

namespace xmod.schema.xmod;

public class Vector2 : ITextDeserializable, IVector2 {
  public Vector2() { }

  public Vector2(float x, float y) {
    this.X = x;
    this.Y = y;
    }

  public float X { get; set; }
  public float Y { get; set; }

  public void Read(ITextReader tr) {
      var values = tr.ReadSingles(TextReaderConstantsExtra.WHITESPACE_STRINGS,
                                  TextReaderConstants.NEWLINE_STRINGS);
      Asserts.Equal(2, values.Length);
      this.X = values[0];
      this.Y = values[1];
    }
}