using schema.binary;
using schema.binary.attributes;

using ttyd.schema.world.chunks;

namespace ttyd.schema.world;

[BinarySchema]
public sealed partial class World : IBinaryDeserializable {
  [WSizeOfStreamInBytes]
  private uint fileSize_;

  private uint mainDataSize_;
  private uint pointerFixupTableCount_;
  
  public uint NamedChunkTableCount { get; private set; }

  [Skip]
  public uint MainDataOffset => 0x20;

  [Skip]
  public uint PointerFixupTableOffset
    => this.MainDataOffset + this.mainDataSize_;

  [Skip]
  public uint NamedChunkTableOffset
    => this.PointerFixupTableOffset + 4 * this.pointerFixupTableCount_;

  [RAtPosition(nameof(NamedChunkTableOffset))]
  public NamedChunkTable NameChunkTable { get; } = new();
}