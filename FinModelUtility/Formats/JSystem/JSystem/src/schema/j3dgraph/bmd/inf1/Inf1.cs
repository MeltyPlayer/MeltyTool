using fin.schema;
using fin.schema.data;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd.inf1;

[BinarySchema]
[LocalPositions]
[Endianness(Endianness.BigEndian)]
public partial class Inf1 : IBinaryConvertible {
  private readonly AutoStringMagicUInt32SizedSection<Inf1Data> impl_ =
      new("INF1") {TweakReadSize = -8};

  [Skip]
  public Inf1Data Data => this.impl_.Data;
}

[BinarySchema]
public partial class Inf1Data : IBinaryConvertible {
  public ushort ScalingRule;
  private readonly ushort padding_ = ushort.MaxValue;

  [Unknown]
  public uint Unknown2;

  public uint NrVertex;

  [WPointerTo(nameof(Entries))]
  private uint entryoffset_;

  [RSequenceUntilEndOfStream]
  [RAtPosition(nameof(entryoffset_))]
  public Inf1Entry[] Entries { get; set; }
}