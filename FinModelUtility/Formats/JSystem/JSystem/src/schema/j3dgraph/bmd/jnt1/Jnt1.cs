using fin.schema.data;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd.jnt1;

[BinarySchema]
[LocalPositions]
[Endianness(Endianness.BigEndian)]
public partial class Jnt1 : IBinaryConvertible {
  private readonly AutoStringMagicUInt32SizedSection<Jnt1Data> impl_ =
      new("JNT1") {TweakReadSize = -8};

  [Skip]
  public Jnt1Data Data => this.impl_.Data;
}

[BinarySchema]
public partial class Jnt1Data : IBinaryConvertible {
  [WLengthOfSequence(nameof(Joints))]
  [WLengthOfSequence(nameof(RemapTable))]
  private ushort jointCount_;

  private readonly ushort padding_ = ushort.MaxValue;

  [WPointerTo(nameof(Joints))]
  private uint jointEntryOffset_;

  [WPointerTo(nameof(RemapTable))]
  private uint remapTableOffset_;

  [WPointerTo(nameof(StringTable))]
  private uint stringTableOffset_;

  [RSequenceLengthSource(nameof(jointCount_))]
  [RAtPosition(nameof(jointEntryOffset_))]
  public Jnt1Entry[] Joints { get; set; }

  [RSequenceLengthSource(nameof(jointCount_))]
  [RAtPosition(nameof(remapTableOffset_))]
  public ushort[] RemapTable { get; set; }

  [RAtPosition(nameof(stringTableOffset_))]
  public StringTable StringTable { get; } = new();
}