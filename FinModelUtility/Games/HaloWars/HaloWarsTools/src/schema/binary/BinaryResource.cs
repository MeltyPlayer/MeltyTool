using System.Linq;

using schema.binary;
using schema.binary.attributes;

namespace hw.schema.binary;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class BinaryResource : IBinaryDeserializable {
  private uint unk0;

  public uint HeaderSize { get; set; }

  private uint unk1;
  private uint unk2;

  [WLengthOfSequence(nameof(Chunks))]
  public ushort NumChunks { get; set; }

  [Skip]
  private uint LengthOfUnk3_ => this.HeaderSize - 18;

  [RSequenceLengthSource(nameof(LengthOfUnk3_))]
  private byte[] unk3;

  [RSequenceLengthSource(nameof(NumChunks))]
  public BinaryResourceChunk[] Chunks { get; private set; }

  public BinaryResourceChunk[] GetAllChunksOfType(
      BinaryResourceChunkType type)
    => this.Chunks.Where(chunk => chunk.Type == type).ToArray();

  public BinaryResourceChunk GetFirstChunkOfType(
      BinaryResourceChunkType type)
    => this.Chunks.FirstOrDefault(chunk => chunk.Type == type);
}