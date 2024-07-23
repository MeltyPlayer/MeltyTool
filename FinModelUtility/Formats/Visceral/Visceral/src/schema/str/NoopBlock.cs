using schema.binary;
using schema.binary.attributes;

namespace visceral.schema.str;

[BinarySchema]
public partial class NoopBlock(BlockType type) : IBlock {
  [Skip]
  public BlockType Type { get; } = type;
}