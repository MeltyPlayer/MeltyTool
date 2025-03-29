using fin.image;

using ICSharpCode.SharpZipLib.BZip2;

using schema.binary;

namespace gm.schema.dataWin.chunk.txtr;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/puggsoy/GMS-Explorer/blob/master/GMS%20Explorer/ChunkItems/SpriteSheet.cs#L11
/// </summary>
public class SpriteSheet : IBinaryDeserializable {
  public IImage Image { get; set; }

  public void Read(IBinaryReader br) {
    var unk0 = br.ReadUInt32();
    var unk1 = br.ReadUInt32();
    var fileLength = br.ReadUInt32();
    var unk3 = br.ReadUInt32();
    var unk4 = br.ReadUInt32();
    var unk5 = br.ReadUInt32();
    var pngOffset = br.ReadUInt32();

    var tmp = br.Position;
    br.Position = pngOffset;

    this.Image = ReadImage_(br, fileLength);

    br.Position = tmp;
  }

  private static IImage ReadImage_(IBinaryReader br, uint fileLength) {
    var header = br.ReadChars(8);
    br.Position -= 8;

    if (header.AsSpan().Slice(0, 4) is "2zoq") {
      return ReadBz2QoiImage_(br, fileLength);
    }

    throw new NotImplementedException();
  }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/UnderminersTeam/UndertaleModTool/blob/f9c17d7116cbc17e63f178350addaeb963ff50c2/UndertaleModLib/Util/GMImage.cs#L36
  /// </summary>
  private static IImage ReadBz2QoiImage_(IBinaryReader br,
                                         uint fileLength) {
    br.Position += 12;
    var compressedLength = fileLength - 12;
    var compressedBytes = br.ReadBytes(compressedLength);

    using (MemoryStream uncompressedData = new()) {
      using (MemoryStream compressedData = new(compressedBytes)) {
        BZip2.Decompress(compressedData, uncompressedData, false);
      }

      // Convert to raw image data
      uncompressedData.Seek(0, SeekOrigin.Begin);

      return QoiConverter.GetImageFromStream(uncompressedData);
    }
  }
}