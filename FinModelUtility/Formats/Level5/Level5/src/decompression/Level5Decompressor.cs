using fin.decompression;

namespace level5.decompression;

public class Level5Decompressor : BIArrayToArrayDecompressor {
  public override bool TryDecompress(byte[] src, out byte[] dst) {
      int tableType = (src[0] & 0xFF);

      DecompressionUtils.GetLengthAndType(src,
                                          out _,
                                          out var decompressionType);

      if (new ZlibArrayToArrayDecompressor().TryDecompress(src, out dst)) {
        return true;
      }

      if (decompressionType == DecompressionType.NOT_COMPRESSED) {
        dst = src.AsSpan(4).ToArray();
      } else {
        ISpanDecompressor decompressor = decompressionType switch {
            DecompressionType.LZSS => new LzssDecompressor(),
            DecompressionType.HUFFMAN_ARRAY_24 => new HuffmanArrayDecompressor(
                0x24),
            DecompressionType.HUFFMAN_ARRAY_28 => new HuffmanArrayDecompressor(
                0x28),
            DecompressionType.RLE_ARRAY => new RleArrayDecompressor(),
        };

        dst = decompressor.Decompress(src);
      }

      return true;
    }
}