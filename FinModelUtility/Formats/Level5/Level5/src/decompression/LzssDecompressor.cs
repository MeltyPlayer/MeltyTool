using System.Runtime.CompilerServices;

using fin.decompression;

namespace level5.decompression;

public sealed class LzssDecompressor : ISpanDecompressor {
  public bool TryToGetLength(ReadOnlySpan<byte> src, out int length) {
      DecompressionUtils.GetLengthAndType(src,
                                          out length,
                                          out var decompressionType);
      return decompressionType == DecompressionType.LZSS;
    }

  public bool TryToDecompressInto(ReadOnlySpan<byte> src, Span<byte> dst) {
      var dstIndex = 0;

      int srcIndex = 4;
      int op = 0;

      int mask = 0;
      int flag = 0;

      while (srcIndex < src.Length && dstIndex < dst.Length) {
        if (mask == 0) {
          flag = src[srcIndex++];
          mask = 0x80;
        }

        if ((flag & mask) == 0) {
          if (srcIndex + 1 > src.Length) break;
          dst[dstIndex++] = src[srcIndex++];
          op++;
        } else {
          if (srcIndex + 2 > src.Length) break;
          var dat = (src[srcIndex++] << 8) | src[srcIndex++];
          int pos = (dat & 0x0FFF) + 1;

          this.Copy_(dst, ref dstIndex, dat, ref op, pos);
        }

        mask >>= 1;
      }

      return true;
    }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void Copy_(
      Span<byte> dst,
      ref int dstIndex,
      int dat,
      ref int op,
      int pos
  ) {
      if (op < pos) {
        return;
      }

      int length = (dat >> 12) + 3;
      for (int i = 0; i < length; i++) {
        if (dstIndex >= dst.Length) {
          return;
        }

        dst[dstIndex] = dst[op - pos >= dstIndex ? 0 : op - pos];
        dstIndex++;
        op++;
      }
    }
}