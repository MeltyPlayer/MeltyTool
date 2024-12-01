using fin.math;
using fin.schema;
using fin.util.strings;

using schema.binary;

using Asserts = fin.util.asserts.Asserts;

namespace fin.compression;

public class LzssDecompressor {
  public bool TryToDecompress(IBinaryReader br, out byte[]? data) {
    if (br.TryReadNew(out LzssHeader? header)) {
      Asserts.Equal(br.Length, 0x10 + header!.CompressedSize);
      data = new byte[header.DecompressedSize];
      var dI = 0;

      Span<byte> buffer = stackalloc byte[4096];
      ushort writeIndex = 0xFEE;
      while (!br.Eof) {
        var flags8 = br.ReadByte();

        for (var i = 0; i < 8; i++) {
          if (flags8.GetBit(0)) {
            var decompressedByte = br.ReadByte();
            data[dI++] = decompressedByte;
            buffer[writeIndex] = decompressedByte;
            writeIndex++;
            writeIndex %= 4096;
          } else {
            var decompressedByte = br.ReadByte();
            ushort readIndex = decompressedByte;

            var someByte = br.ReadByte();
            readIndex |= (ushort) ((someByte & 0xF0) << 4);
            for (var j = 0; j < (someByte & 0x0F) + 3; j++) {
              data[dI++] = buffer[readIndex];
              buffer[writeIndex] = buffer[readIndex];
              readIndex++;
              readIndex %= 4096;
              writeIndex++;
              writeIndex %= 4096;
            }
          }

          flags8 >>= 1;
          if (br.Eof) {
            break;
          }
        }
      }

      Asserts.Equal(header.DecompressedSize, (uint) dI);

      return true;
    }

    data = null;
    return false;
  }
}

[BinarySchema]
public partial class LzssHeader : IBinaryConvertible {
  private readonly string magic_ = "LzS" + AsciiUtil.GetChar(0x1);

  [Unknown]
  public uint Unknown { get; set; }

  public uint DecompressedSize { get; set; }
  public uint CompressedSize { get; set; }
}