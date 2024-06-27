namespace level5.decompression;

public enum DecompressionType : byte {
  NOT_COMPRESSED = 0,
  LZSS = 1,
  HUFFMAN_ARRAY_24 = 2,
  HUFFMAN_ARRAY_28 = 3,
  RLE_ARRAY = 4,
}

public static class DecompressionUtils {
  public static void GetLengthAndType(
      ReadOnlySpan<byte> src,
      out int length,
      out DecompressionType decompressionType) {
      var b = src;
      var firstUint = (b[0] & 0xFF) | ((b[1] & 0xFF) << 8) |
                      ((b[2] & 0xFF) << 16) | ((b[3] & 0xFF) << 24);

      length = firstUint >> 3;
      decompressionType = (DecompressionType) (firstUint & 0x7);
    }
}