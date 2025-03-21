using fin.io;
using fin.util.asserts;

using schema.binary;

namespace gx.compression.yay0;

public class Yay0Dec {
  public bool Run(IFileHierarchyFile srcFile,
                  ISystemFile dstFile,
                  bool cleanup) {
    Asserts.True(
        srcFile.Exists,
        $"Cannot decrypt YAY0 because it does not exist: {srcFile}");

    if (dstFile.Exists) {
      return false;
    }

    using var src = srcFile.OpenRead();
    using var dst = dstFile.OpenWrite();
    Decompress_(src, dst);

    if (cleanup) {
      srcFile.Impl.Delete();
    }

    return true;
  }

  private static void Decompress_(Stream src, Stream dst) {
    using var srcBr = new SchemaBinaryReader(src, Endianness.BigEndian);

    srcBr.AssertString("Yay0");

    var uncompSize = srcBr.ReadUInt32();
    var linkOffset = srcBr.ReadUInt32();
    var chunkOffset = srcBr.ReadUInt32();

    var dstBuffer = new MemoryStream((int) uncompSize);

    uint outputLen = 0;
    var maskBitsLeft = 0;
    uint mask = 0;
    while (outputLen < uncompSize) {
      if (maskBitsLeft == 0) {
        mask = srcBr.ReadUInt32();
        maskBitsLeft = 32;
      }

      if ((mask & 0x80000000) != 0) {
        var tmp = srcBr.Position;
        srcBr.Position = chunkOffset;

        dstBuffer.WriteByte(srcBr.ReadByte());
        ++chunkOffset;
        ++outputLen;

        srcBr.Position = tmp;
      } else {
        var tmp = srcBr.Position;
        srcBr.Position = linkOffset;

        var link = srcBr.ReadUInt16();
        linkOffset += 2;

        srcBr.Position = tmp;

        var dist = link & 0x0FFF;
        uint copy_src_offset = (uint) (outputLen - (dist + 1));
        var num_bytes = link >> 12;

        if (num_bytes == 0) {
          var chunkTmp = srcBr.Position;
          srcBr.Position = chunkOffset;

          num_bytes = srcBr.ReadByte() + 0x12;
          ++chunkOffset;

          srcBr.Position = chunkTmp;
        } else {
          num_bytes += 2;
        }

        CopyFromEarlierInStream_(dstBuffer, copy_src_offset, num_bytes);
        outputLen += (uint) num_bytes;
      }

      mask <<= 1;
      --maskBitsLeft;
    }

    dstBuffer.Position = 0;
    dstBuffer.CopyTo(dst);
  }

  private static void CopyFromEarlierInStream_(
      Stream stream,
      uint srcOffset,
      int srcLength) {
    Span<byte> buffer = stackalloc byte[srcLength];

    var tmp = stream.Position;
    stream.Position = srcOffset;
    stream.Read(buffer);
    stream.Position = tmp;

    stream.Write(buffer);
  }
}