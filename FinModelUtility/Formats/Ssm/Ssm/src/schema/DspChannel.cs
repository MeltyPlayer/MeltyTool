using schema.binary;
using schema.binary.attributes;

namespace ssm.schema;

[BinarySchema]
public partial class DspChannel : IBinaryDeserializable, IChildOf<Dsp> {
  public Dsp Parent { get; set; }

  public ushort LoopFlag { get; set; }
  public ushort Format { get; set; }

  public uint LoopStartOffset { get; set; }
  public uint LoopEndOffset { get; set; }
  public uint CurrentAddress { get; set; }

  [SequenceLengthSource(0x10)]
  public ushort[] Coefficients { get; set; }

  public short Gain { get; set; }
  public short InitialPredictorScale { get; set; }
  public short InitialSampleHistory1 { get; set; }
  public short InitialSampleHistory2 { get; set; }
  public short LoopPredictorScale { get; set; }
  public short LoopSampleHistory1 { get; set; }
  public short LoopSampleHistory2 { get; set; }
  private ushort padding_;

  [Skip]
  public uint DataOffset
    => this.Parent.Parent.HeaderLengthMinus16 +
       0x10 +
       (this.CurrentAddress >> 1) -
       1;

  [Skip]
  public uint DataLength => (this.LoopEndOffset - this.CurrentAddress) >> 2 + 1;

  [RAtPosition(nameof(DataOffset))]
  [RSequenceLengthSource(nameof(DataLength))]
  public byte[] Bytes { get; set; }
}