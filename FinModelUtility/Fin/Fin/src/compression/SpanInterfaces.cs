using System;

namespace fin.compression;

public interface ISpanDecompressor {
  bool TryToGetLength(ReadOnlySpan<byte> src, out int length);
  bool TryToDecompressInto(ReadOnlySpan<byte> src, Span<byte> dst);
}

public static class SpanDecompressorExtensions {
  public static bool TryToDecompress(this ISpanDecompressor decompressor,
                                     ReadOnlySpan<byte> src,
                                     out byte[] dst) {
    dst = default;
    if (!decompressor.TryToGetLength(src, out var length)) {
      return false;
    }

    dst = new byte[length];
    return decompressor.TryToDecompressInto(src, dst);
  }

  public static byte[] Decompress(this ISpanDecompressor decompressor,
                                  ReadOnlySpan<byte> src) {
    if (decompressor.TryToDecompress(src, out byte[] dst)) {
      return dst;
    }

    throw new Exception("Failed to decompress bytes.");
  }
}