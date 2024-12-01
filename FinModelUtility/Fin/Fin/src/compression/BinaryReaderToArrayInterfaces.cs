using System;

using schema.binary;

namespace fin.compression;

public interface IBinaryReaderToArrayDecompressor {
  bool TryDecompress(IBinaryReader br, out byte[] dst);
  byte[] Decompress(IBinaryReader br);
}

public abstract class BBinaryReaderToArrayDecompressor
    : IBinaryReaderToArrayDecompressor {
  public abstract bool TryDecompress(IBinaryReader br, out byte[] dst);

  public byte[] Decompress(IBinaryReader br) {
    if (this.TryDecompress(br, out byte[] dst)) {
      return dst;
    }

    throw new Exception("Failed to decompress bytes.");
  }
}