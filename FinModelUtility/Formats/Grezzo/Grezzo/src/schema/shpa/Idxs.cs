using schema.binary;
using schema.binary.attributes;


namespace grezzo.schema.shpa;

[BinarySchema]
public partial class Idxs : IBinaryConvertible, IChildOf<Shpa> {
  public Shpa Parent { get; set; }

  /// <summary>
  ///   The corresponding indices in the original model to update?
  /// </summary>
  [RSequenceLengthSource(nameof(Parent.IdxsCount))]
  public ushort[] Indices { get; private set; }

  [Align(4)]
  [SequenceLengthSource((uint) 0)]
  private byte[] endAlignment_;
}