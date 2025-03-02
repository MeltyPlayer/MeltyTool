using System.Numerics;

using schema.text;
using schema.text.reader;


namespace xmod.schema.skel;

public class SkelBone : ITextDeserializable {
  public string Name { get; set; }
  public Vector3 Offset { get; set; }
  public LinkedList<SkelBone> Children { get; } = new();

  public void Read(ITextReader tr) {
    {
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      tr.AssertString("bone");

      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      this.Name
          = tr.ReadUpToStartOfTerminator(TextReaderConstants.WHITESPACE_CHARS);
    }

    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    tr.AssertChar('{');

    {
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      tr.AssertString("offset");

      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      this.Offset = new Vector3(tr.ReadSingles(
                                    TextReaderConstants.WHITESPACE_CHARS,
                                    TextReaderConstants.NEWLINE_CHARS));
    }

    tr.ReadUpToStartOfTerminator(["bone", "}"]);

    this.Children.Clear();
    while (!tr.Matches('}')) {
      this.Children.AddLast(tr.ReadNew<SkelBone>());

      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    }
  }
}

public class Skel : ITextDeserializable {
  public uint Version { get; set; }
  public SkelBone Root { get; set; }

  public void Read(ITextReader tr) {
    tr.AssertString("Version: ");
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    this.Version = tr.ReadUInt32();

    tr.AssertString("NumBones ");
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    tr.ReadUInt32();

    this.Root = tr.ReadNew<SkelBone>();
  }
}