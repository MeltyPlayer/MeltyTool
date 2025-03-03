using System.Numerics;

using schema.text;
using schema.text.reader;


namespace xmod.schema.skel;

public class BoneCounter {
  private int currentId_;
  public int GetAndIncrement() => this.currentId_++;
}

public class SkelBone(BoneCounter boneCounter) : ITextDeserializable {


  public int Id { get; set; }
  public string Name { get; set; }
  public Vector3 Offset { get; set; }
  public LinkedList<SkelBone> Children { get; } = new();

  public void Read(ITextReader tr) {
    {
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      tr.AssertString("bone");
      this.Id = boneCounter.GetAndIncrement();

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
      var child = new SkelBone(boneCounter);
      child.Read(tr);
      this.Children.AddLast(child);

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

    var root = new SkelBone(new BoneCounter());
    root.Read(tr);
    this.Root = root;
  }
}