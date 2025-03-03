using fin.schema;
using fin.util.asserts;

using schema.text;
using schema.text.reader;


namespace xmod.schema.xmod;

public class Adjunct : ITextDeserializable {
  public int PositionIndex { get; set; }
  public int NormalIndex { get; set; }
  public int ColorIndex { get; set; }
  public int Uv1Index { get; set; }
  public int MatrixIndex { get; set; }

  public void Read(ITextReader tr) {
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    tr.AssertString("adj");

    var indices = tr.ReadInt32s(TextReaderConstants.WHITESPACE_STRINGS,
                                TextReaderConstants.NEWLINE_STRINGS);
    Asserts.Equal(6, indices.Length);

    this.PositionIndex = indices[0];
    this.NormalIndex = indices[1];
    this.ColorIndex = indices[2];
    this.Uv1Index = indices[3];

    // TODO: This might not be correct
    var uv2Index = indices[4];

    this.MatrixIndex = indices[5];

    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
  }
}