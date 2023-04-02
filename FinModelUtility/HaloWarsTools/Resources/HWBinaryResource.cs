using System;
using System.Linq;


namespace HaloWarsTools {
  public abstract class HWBinaryResource : HWResource {
    protected HWBinaryResourceChunk[] Chunks { get; private set; }

    protected override void Load(byte[] bytes) {
      uint headerSize = BinaryUtils.ReadUInt32BigEndian(bytes, 4);
      ushort numChunks = BinaryUtils.ReadUInt16BigEndian(bytes, 16);
      int chunkHeaderSize = 24;

      var chunks = new HWBinaryResourceChunk[numChunks];
      for (int i = 0; i < chunks.Length; i++) {
        int offset = (int) headerSize + i * chunkHeaderSize;

        chunks[i] = new HWBinaryResourceChunk() {
            Type = ParseChunkType(
                BinaryUtils.ReadUInt64BigEndian(bytes, offset)),
            Offset = BinaryUtils.ReadUInt32BigEndian(bytes, offset + 8),
            Size = BinaryUtils.ReadUInt32BigEndian(bytes, offset + 12)
        };
      }

      this.Chunks = chunks;
    }

    protected static HWBinaryResourceChunkType ParseChunkType(ulong type) {
      if (Enum.TryParse(type.ToString(),
                        out HWBinaryResourceChunkType result)) {
        return result;
      }

      return HWBinaryResourceChunkType.Unknown;
    }

    protected HWBinaryResourceChunk[] GetAllChunksOfType(
        HWBinaryResourceChunkType type)
      => Chunks.Where(chunk => chunk.Type == type).ToArray();

    protected HWBinaryResourceChunk GetFirstChunkOfType(
        HWBinaryResourceChunkType type)
      => Chunks.FirstOrDefault(chunk => chunk.Type == type);
  }
}