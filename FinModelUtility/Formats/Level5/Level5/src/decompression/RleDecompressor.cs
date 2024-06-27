using fin.decompression;

namespace level5.decompression;

public class RleArrayDecompressor : ISpanDecompressor {
  public bool TryToGetLength(ReadOnlySpan<byte> src, out int length) {
      DecompressionUtils.GetLengthAndType(src,
                                          out length,
                                          out var decompressionType);
      return decompressionType == DecompressionType.RLE_ARRAY;
    }

  public bool TryToDecompressInto(ReadOnlySpan<byte> src, Span<byte> dst) {
      var p = 4;
      var dstIndex = 0;

      while (p < src.Length && dstIndex < dst.Length) {
        int flag = (byte) src[p++];

        bool compressed = (flag & 0x80) > 0;
        int length = flag & 0x7F;

        if (compressed)
          length += 3;
        else
          length += 1;

        if (compressed) {
          int data = (byte) src[p++];

          byte bdata = (byte) data;
          for (int i = 0; i < length; i++) {
            if (dstIndex > dst.Length) {
              break;
            }

            dst[dstIndex++] = bdata;
          }
        } else {
          int tryReadLength = length;
          for (int i = 0; i < tryReadLength; i++) {
            if (dstIndex > dst.Length) {
              break;
            }

            dst[dstIndex++] = (byte) (src[p++] & 0xFF);
          }
        }
      }

      return true;
    }
}