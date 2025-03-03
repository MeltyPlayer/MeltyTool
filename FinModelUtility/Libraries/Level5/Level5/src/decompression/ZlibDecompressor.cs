using System.IO.Compression;

using fin.compression;

namespace level5.decompression;

public class ZlibArrayToArrayDecompressor : BArrayToArrayDecompressor {
  public override bool TryDecompress(byte[] src, out byte[] dst) {
      var b = src;
      if (b.Length < 6) {
        dst = null;
        return false;
      }

      if (b[4] != 0x78) {
        dst = null;
        return false;
      }

      var decomLength = (b[0] & 0xFF) | ((b[1] & 0xFF) << 8) |
                        ((b[2] & 0xFF) << 16) | ((b[3] & 0xFF) << 24);
      var data = new byte[b.Length - 4];
      b.AsSpan(4).CopyTo(data);

      var stream = new MemoryStream();
      var ms = new MemoryStream(data);
      ms.ReadByte();
      ms.ReadByte();
      var zlibStream = new DeflateStream(ms, CompressionMode.Decompress);
      Span<byte> buffer = stackalloc byte[2048];
      while (true) {
        int size = zlibStream.Read(buffer);
        if (size > 0)
          stream.Write(buffer.Slice(0, size));
        else
          break;
      }

      zlibStream.Close();
      dst = stream.ToArray();
      return true;
    }
}