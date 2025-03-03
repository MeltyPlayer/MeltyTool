using schema.binary;

namespace modl.schema.terrain;

[BinarySchema]
public partial class TerrData : IBinaryConvertible {
  public int ChunkCountX { get; } = 64;
  public int ChunkCountY { get; } = 64;

  public uint SomeCount { get; } = 1;

  public int MaterialCount { get; private set; }
}