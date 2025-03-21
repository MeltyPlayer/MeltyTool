using fin.io;
using fin.util.asserts;
using fin.util.streams;

using schema.binary;

namespace gx.compression.yaz0;

public class Yaz0Dec {
  public bool Run(IFileHierarchyFile srcFile, bool cleanup)
    => this.Run(srcFile, srcFile.Impl.CloneWithFileType(".rarc"), cleanup);

  public bool Run(IFileHierarchyFile srcFile,
                  ISystemFile dstFile,
                  bool cleanup) {
    Asserts.True(
        srcFile.Exists,
        $"Cannot decrypt YAZ0 because it does not exist: {srcFile}");

    if (dstFile.Exists) {
      return false;
    }

    if (!MagicTextUtil.Verify(srcFile, "Yaz0")) {
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
  ///   https://github.com/LagoLunatic/gclib/blob/3a8a9339cc63c1d57490e9c094b0a50daa7ba736/gclib/yaz0_yay0.py#L98
  /// </summary>
  private static void Decompress_(Stream src, Stream dst) {
    using var srcBr = new SchemaBinaryReader(src, Endianness.BigEndian);

    srcBr.AssertString("Yaz0");

    var uncompSize = srcBr.ReadUInt32();
    srcBr.Position = 0x10;

    var dstBuffer = new MemoryStream((int) uncompSize);
    
    var maskBitsLeft = 0;
    uint mask = 0;
    while (dstBuffer.Length < uncompSize) {
      if (maskBitsLeft == 0) {
        mask = srcBr.ReadByte();
        maskBitsLeft = 8;
      }

      if ((mask & 0x80) != 0) {
        dstBuffer.WriteByte(srcBr.ReadByte());
      } else {
        var byte1 = srcBr.ReadByte();
        var byte2 = srcBr.ReadByte();

        var dist = ((byte1 & 0xF) << 8) | byte2;
        var copySrcOffset = (uint) (dstBuffer.Length - (dist + 1));
        var numBytes = byte1 >> 4;

        if (numBytes == 0) {
          numBytes = srcBr.ReadByte() + 0x12;
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