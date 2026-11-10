using fin.util.asserts;

using LIBMIO0;

using schema.binary;

namespace bar.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/Filesystem.ts#L123
/// </summary>
public sealed partial class FileChunks : IBinaryDeserializable {
  public IReadOnlyList<UvFileChunk> Chunks { get; set; }

  public void Read(IBinaryReader br) {
    var chunks = new List<UvFileChunk>();
    this.Chunks = chunks;

    if (br.Eof) {
      return;
    }

    br.AssertString("FORM");
    br.Position += 4; // Size of file
    br.Position += 4; // Magic for file

    while (!br.Eof) {
      var chunkTag = br.ReadString(4);
      var chunkLength = br.ReadUInt32();

      switch (chunkTag) {
        case "GZIP": {
          var realChunkTag = br.ReadString(4);
          var decompressedLength = br.ReadUInt32();

          var decompressed = MIO0.mio0_decode(br.ReadBytes(chunkLength - 8));
          Asserts.Equal(decompressedLength, (uint) decompressed!.Length);
          chunks.Add(new UvFileChunk(realChunkTag, decompressed));

          break;
        }
        case "PAD ": {
          var padData = br.ReadBytes(chunkLength);
          //chunks.Add(new UvFileChunk(chunkTag, padData));
          break;
        }
        default: {
          var commData = br.ReadBytes(chunkLength);
          chunks.Add(new UvFileChunk(chunkTag, commData));
          break;
        }
      }
    }
  }
}

public record UvFileChunk(string Tag, byte[] Buffer);