using schema.binary;

namespace grezzo.schema.cmb.luts {

  [BinarySchema]
  public partial class LutKeyframe : IBinaryConvertible {
    public float InSlope;
    public float OutSlope;
    public int Frame;
    public float Value;
  }
}
