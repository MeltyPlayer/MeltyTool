using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.mainfs;

[BinarySchema]
public sealed partial class MainFsFile : IBinaryDeserializable {
  public uint DecompressedSize { get; set; }
  public uint CompressionType { get; set; }

  [Skip]
  public byte[] DecompressedBytes { get; private set; }

  [ReadLogic]
  private void Decompress_(IBinaryReader br) {
    switch (this.CompressionType) {
      case 0: {
        this.DecompressedBytes = br.ReadBytes(this.DecompressedSize);
        break;
      }
      case 1: {
        this.DecompressedBytes = new byte[this.DecompressedSize];
        DecompressionUtil.Decompress01(br, this.DecompressedBytes);
        break;
      }
      case 2 or 4: {
        this.DecompressedBytes = new byte[this.DecompressedSize];
        var unk = br.ReadUInt32();
        DecompressionUtil.Decompress02(br, this.DecompressedBytes);
        break;
      }
      case 5: {
        this.DecompressedBytes = new byte[this.DecompressedSize];
        DecompressionUtil.Decompress05(br, this.DecompressedBytes);
        break;
      }
    }
  }
}