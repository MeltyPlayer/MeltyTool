using schema.binary;
using schema.binary.attributes;

namespace fin.schema;

[BinarySchema]
public sealed partial class RPointer<T> : IBinaryDeserializable
    where T : IBinaryDeserializable, new() {
  private uint offset_;

  [RAtPosition(nameof(offset_))]
  public T Value { get; } = new();
}