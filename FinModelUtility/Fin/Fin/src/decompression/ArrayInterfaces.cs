using System;

namespace fin.decompression;

public interface IArrayDecompressor {
  bool TryDecompress(byte[] src, out byte[] dst);

  byte[] Decompress(byte[] src);
}

public abstract class BArrayDecompressor : IArrayDecompressor {
  public abstract bool TryDecompress(byte[] src, out byte[] dst);

  public byte[] Decompress(byte[] src) {
    if (this.TryDecompress(src, out byte[] dst)) {
      return dst;
    }

    throw new Exception("Failed to decompress bytes.");
  }
}