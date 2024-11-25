using fin.schema;
using fin.schema.data;

using grezzo.schema.shpa.norm;
using grezzo.schema.shpa.posi;

using schema.binary;
using schema.binary.attributes;


namespace grezzo.schema.shpa;

[BinarySchema]
[Endianness(Endianness.LittleEndian)]
[AlignEnd(0x10)]
public partial class Shpa : IBinaryConvertible {
  private readonly string magic_ = "shpa";
  private readonly uint headerLength_ = 48;

  [Unknown]
  public uint unk0;

  private readonly uint animationCount_ = 1;

  [StringLengthSource(16)]
  public string Name { get; set; }

  [Unknown]
  public uint IdxsCount { get; private set; }


  [WPointerTo(nameof(Posi))]
  private uint posiOffset_;

  [WPointerTo(nameof(Norm))]
  private uint normOffset_;

  [WPointerTo(nameof(IdxsSection))]
  private uint idxsOffset_;


  [RAtPosition(nameof(posiOffset_))]
  public AutoStringMagicUInt32SizedSection<Posi> Posi { get; } =
    new("posi") {TweakReadSize = -8,};

  [RAtPosition(nameof(normOffset_))]
  public AutoStringMagicUInt32SizedSection<Norm> Norm { get; } =
    new("norm") {TweakReadSize = -8,};

  [ReadLogic]
  private void FixParent_(IBinaryReader _) {
    this.IdxsSection.Data.Parent = this;
  }

  [RAtPosition(nameof(idxsOffset_))]
  public AutoStringMagicJankSizedSection<Idxs> IdxsSection { get; } =
    new("idxs") {TweakReadSize = -8};
}