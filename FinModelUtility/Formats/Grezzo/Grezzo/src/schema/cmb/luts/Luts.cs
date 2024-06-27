using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.luts;

/// <summary>
///   "LUT" stands for "lookup table". (But where is this actually used...?)
/// </summary>
[BinarySchema]
public partial class Luts : IBinaryConvertible {
  [WLengthOfSequence(nameof(Offset))]
  [WLengthOfSequence(nameof(luts))]
  private uint lutSetCount_;

  [Unknown]
  public uint unk;

  [RSequenceLengthSource(nameof(lutSetCount_))]
  public uint[] Offset { get; set; }

  [RSequenceLengthSource(nameof(lutSetCount_))]
  public LutSet[] luts { get; set; }
}