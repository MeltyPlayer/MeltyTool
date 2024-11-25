using schema.binary;
using schema.binary.attributes;


namespace grezzo.schema.shpa;

[BinarySchema]
[AlignEnd(0x4)]
public partial class Idxs : IBinaryConvertible, IChildOf<Shpa> {
  public Shpa Parent { get; set; }

  /// <summary>
  ///   The corresponding indices in the original model to update?
  /// </summary>
  [RSequenceLengthSource(nameof(Parent.IdxsCount))]
  public ushort[] Indices { get; private set; }
}