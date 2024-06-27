using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.shpa;

[BinarySchema]
public partial class Idxs : IBinaryConvertible {
  private readonly string magic_ = "idxs";

  /// <summary>
  ///   The corresponding indices in the original model to update?
  /// </summary>
  [SequenceLengthSource(SchemaIntegerType.INT32)]
  public ushort[] Indices { get; private set; }
}