using System.Linq;

using fin.schema;
using fin.schema.sequences;
using fin.util.enumerables;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd;

[BinarySchema]
public partial class StringTable : IBinaryConvertible {
  [WLengthOfSequence(nameof(EntriesAndStrings))]
  private ushort entryCount_;

  private readonly ushort padding_ = ushort.MaxValue;

  [RSequenceLengthSource(nameof(entryCount_))]
  public ConsecutiveLists2<StringTableEntry, StringTableString>
      EntriesAndStrings { get; } = new();

  public string this[int index]
    => this.EntriesAndStrings[index].Second.String;

  public int this[string value] =>
      this.EntriesAndStrings.Select(entry => entry.Second.String)
          .IndexOfOrNegativeOne(value);
}

[BinarySchema]
public partial class StringTableEntry : IBinaryConvertible {
  [Unknown]
  public ushort unknown;

  private ushort offset;
}

[BinarySchema]
public partial class StringTableString : IBinaryConvertible {
  [NullTerminatedString]
  public string String { get; set; }
}