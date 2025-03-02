using schema.text;
using schema.text.reader;


namespace xmod.schema.ped;

public class Ped : ITextDeserializable {
  public string SkelName { get; set; }
  public string[] XmodNames { get; set; }
  public IDictionary<string, string> AnimMap { get; set; }

  public void Read(ITextReader tr) {
    this.SkelName = TextReaderUtils.ReadKeyValue(tr, "skel").Trim();
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

    var xmodNames = new LinkedList<string>();
    while (tr.Matches(out _, ["lod"])) {
      var lodIndex = tr.ReadUInt32();
      tr.ReadUpToAndPastTerminator('{');
      xmodNames.AddLast(
          tr.ReadUpToAndPastTerminator(TextReaderUtils.CLOSING_BRACE).Trim());
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    }

    this.XmodNames = xmodNames.ToArray();

    this.AnimMap = new Dictionary<string, string>();
    tr.AssertString("anim {");
    while (true) {
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      if (tr.Matches('}')) {
        break;
      }

      var key = tr.ReadUpToAndPastTerminator(TextReaderUtils.COLON);
      var value = tr.ReadLine().Trim();
      this.AnimMap[key] = value;
    }
  }
}