using schema.text;
using schema.text.reader;


namespace xmod.schema.xmod;

public class TextureId : ITextDeserializable {
  public int Index { get; set; }
  public string Name { get; set; }

  public void Read(ITextReader tr) {
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    this.Index = tr.ReadInt32();
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    tr.AssertChar('"');
    this.Name = tr.ReadUpToAndPastTerminator(TextReaderUtils.QUOTE);
  }
}