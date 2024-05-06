using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.mats {
  [BinarySchema]
  public partial class Combiner : IBinaryConvertible {
    public TexCombineMode combinerModeColor;
    public TexCombineMode combinerModeAlpha;
    public TexCombineScale scaleColor;
    public TexCombineScale scaleAlpha;
    public TexBufferSource bufferColor;
    public TexBufferSource bufferAlpha;


    [SequenceLengthSource(3)]
    public TexCombinerSource[] colorSources;

    [SequenceLengthSource(3)]
    public TexCombinerColorOp[] colorOperands;


    [SequenceLengthSource(3)]
    public TexCombinerSource[] alphaSources;

    [SequenceLengthSource(3)]
    public TexCombinerAlphaOp[] alphaOperands;


    public int constColorIndex;
  }
}