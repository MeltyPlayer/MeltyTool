using schema.text;
using schema.text.reader;


namespace xmod.schema.ped;

public class Ped : ITextDeserializable {
  public string SkelName { get; set; }
  public string XmodName { get; set; }
  public IDictionary<string, string> AnimMap { get; set; }

  public void Read(ITextReader tr) {
    this.SkelName = TextReaderUtils.ReadKeyValue(tr, "skel").Trim();
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

    tr.AssertString("lod 0 {");
    this.XmodName = tr.ReadUpToAndPastTerminator(TextReaderUtils.CLOSING_BRACE)
                      .Trim();
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

    this.AnimMap = new Dictionary<string, string>();
    tr.AssertString("anim {");
    while (true) {
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      if (tr.Matches(out _, "}")) {
        break;
      }

      var key = tr.ReadUpToAndPastTerminator(TextReaderUtils.COLON);
      var value = tr.ReadLine().Trim();
      this.AnimMap[key] = value;
    }
  }
}