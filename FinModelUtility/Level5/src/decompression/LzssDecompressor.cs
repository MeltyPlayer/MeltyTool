using System.Runtime.CompilerServices;


namespace level5.decompression {
  public class LzssDecompressor {
    public byte[] Decompress(ReadOnlySpan<byte> src) {
      if (TryDecompress(src, out byte[] dst)) {
        return dst;
      }

      throw new Exception("Failed to decompress bytes.");
    }

    public bool TryDecompress(ReadOnlySpan<byte> src,
                              out byte[] dst) {
      var size = 0;
      {
        int srcIndex = 4;
        int op = 0;

        int mask = 0;
        int flag = 0;

        while (srcIndex < src.Length) {
          if (mask == 0) {
            flag = src[srcIndex++];
            mask = 0x80;
          }

          if ((flag & mask) == 0) {
            if (srcIndex + 1 > src.Length) break;
            srcIndex++;
            size++;
            op++;
          } else {
            if (srcIndex + 2 > src.Length) break;


            var dat = (src[srcIndex++] << 8) | src[srcIndex++];
            int pos = (dat & 0x0FFF) + 1;

            if (op >= pos) {
              int length = (dat >> 12) + 3;
              size += length;
              op += length;
            }
          }

          mask >>= 1;
        }
      }

      dst = new byte[size];
      {
        var dstIndex = 0;

        int srcIndex = 4;
        int op = 0;

        int mask = 0;
        int flag = 0;

        while (srcIndex < src.Length) {
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
      if (op >= pos) {
        int length = (dat >> 12) + 3;
        for (int i = 0; i < length; i++) {
          dst[dstIndex] = dst[op - pos >= dstIndex ? 0 : op - pos];
          dstIndex++;
          op++;
        }
      }
    }
  }
}