using System;

using schema.binary;
using schema.binary.attributes;

namespace mod.schema.anm {
  public enum AnimationFormat : uint {
    DCA = 2,
    DCK = 3,
  }

  [BinarySchema]
  public partial class DcxWrapper : IBinaryConvertible {
    public AnimationFormat AnimationFormat { get; set; }

    [Skip]
    private bool IsDca => this.AnimationFormat == AnimationFormat.DCA;

    [RIfBoolean(nameof(IsDca))]
    public Dca? Dca { get; set; }

    [Skip]
    private bool IsDck => this.AnimationFormat == AnimationFormat.DCK;

    [RIfBoolean(nameof(IsDck))]
    public Dck? Dck { get; set; }

    [Skip]
    public IDcx Dcx
      => IsDca ? Dca! :
         IsDck ? Dck! :
         throw new NotSupportedException();

    public override string ToString() => this.Dcx.ToString()!;
  }
}
