using schema.binary;

namespace visceral.schema.str;

public interface IBlock : IBinaryConvertible {
  BlockType Type { get; }
}