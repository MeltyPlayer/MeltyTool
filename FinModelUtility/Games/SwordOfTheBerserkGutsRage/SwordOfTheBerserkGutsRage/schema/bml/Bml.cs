using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace berserkDc.schema.bml;

[BinarySchema]
public sealed partial class Bml : IBinaryDeserializable {
  [WLengthOfSequence(nameof(Entries))]
  private uint entryCount_;

  [Unknown]
  private readonly byte[] unknown_ = new byte[0x40 - 4];

  [RSequenceLengthSource(nameof(entryCount_))]
  public BmlEntry[] Entries { get; set; }
}