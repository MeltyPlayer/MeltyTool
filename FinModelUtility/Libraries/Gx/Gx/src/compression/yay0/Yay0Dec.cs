using fin.io;
using fin.util.asserts;
using fin.util.streams;

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

    if (!MagicTextUtil.Verify(srcFile, "Yay0")) {
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

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/LagoLunatic/gclib/blob/3a8a9339cc63c1d57490e9c094b0a50daa7ba736/gclib/yaz0_yay0.py#L252
  /// </summary>
  private static void Decompress_(Stream src, Stream dst) {
    using var srcBr = new SchemaBinaryReader(src, Endianness.BigEndian);

    srcBr.AssertString("Yay0");

    var uncompSize = srcBr.ReadUInt32();
    var linkOffset = srcBr.ReadUInt32();
    var chunkOffset = srcBr.ReadUInt32();

    var dstBuffer = new MemoryStream((int) uncompSize);

    var maskBitsLeft = 0;
    uint mask = 0;
    while (dstBuffer.Length < uncompSize) {
      if (maskBitsLeft == 0) {
        mask = srcBr.ReadUInt32();
        maskBitsLeft = 32;
      }

      if ((mask & 0x80000000) != 0) {
        var tmp = srcBr.Position;
        srcBr.Position = chunkOffset;

        dstBuffer.WriteByte(srcBr.ReadByte());
        ++chunkOffset;

        srcBr.Position = tmp;
      } else {
        var tmp = srcBr.Position;
        srcBr.Position = linkOffset;

        var link = srcBr.ReadUInt16();
        linkOffset += 2;

        srcBr.Position = tmp;

        var dist = link & 0x0FFF;
        uint copySrcOffset = (uint) (dstBuffer.Length - (dist + 1));
        var numBytes = link >> 12;

        if (numBytes == 0) {
          var chunkTmp = srcBr.Position;
          srcBr.Position = chunkOffset;

          numBytes = srcBr.ReadByte() + 0x12;
          ++chunkOffset;

          srcBr.Position = chunkTmp;
        } else {
          numBytes += 2;
        }

        dstBuffer.CopyFromMiddleToEnd(copySrcOffset, numBytes);
      }

      mask <<= 1;
      --maskBitsLeft;
    }

    dstBuffer.Position = 0;
    dstBuffer.CopyTo(dst);
  }
}