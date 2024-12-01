using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;

using fin.math;
using fin.schema;
using fin.util.asserts;
using fin.util.strings;

using schema.binary;

namespace fin.decompression;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/scurest/apicula/blob/3d4e91e14045392a49c89e86dab8cb936225588c/src/decompress/mod.rs
/// </summary>
public class Lz77Decompressor : BBinaryReaderToArrayDecompressor {
  public override bool TryDecompress(IBinaryReader br, out byte[] data) {
    br.AssertString("LZ77");

    var compressionType = br.ReadByte();
    switch (compressionType) {
      case 0x10: {
        data = Decompress10_(br);
        return true;
      }
      case 0x11: {
        data = Decompress11_(br);
        return true;
      }
    }

    data = default;
    return false;
  }

  private static byte[] Decompress10_(IBinaryReader br) {
    var decompressedSize = ReadDecompressedSize_(br);
    var data = new List<byte>((int) decompressedSize);

    while (data.Count < decompressedSize) {
      var flags = br.ReadByte();

      for (var i = 0; i < 8; ++i) {
        var compressed = (flags & 0x80) != 0;
        flags <<= 1;

        if (!compressed) {
          // Uncompressed byte
          data.Add(br.ReadByte());
        } else {
          // LZ backreference
          var ofsSub1And3 = br.ReadUInt16();
          var ofsSub1 = ofsSub1And3.ExtractFromRight(0, 12);
          var ofsSub3 = ofsSub1And3.ExtractFromRight(12, 4);

          var ofs = ofsSub1 + 1;
          var n = ofsSub3 + 3;

          if (data.Count + n > decompressedSize) {
            Asserts.Fail("Too much data!");
          }

          if (data.Count < ofs) {
            Asserts.Fail("Not enough data!");
          }

          for (var ii = 0; ii < 8; ++ii) {
            var x = data[data.Count - ofs];
            data.Add(x);
          }
        }

        if (data.Count >= decompressedSize) {
          break;
        }
      }
    }

    return data.ToArray();
  }

  private static byte[] Decompress11_(IBinaryReader br) {
    var decompressedSize = ReadDecompressedSize_(br);
    var data = new List<byte>((int) decompressedSize);

    while (data.Count < decompressedSize) {
      var flags = br.ReadByte();
      for (var i = 0; i < 8; ++i) {
        var compressed = (flags & 0x80) != 0;
        flags <<= 1;

        if (!compressed) {
          // Uncompressed byte
          data.Add(br.ReadByte());
        } else {
          br.ReadByte().SplitNibbles(out var a, out var b);
          var cd = br.ReadByte();

          int n, ofs;
          switch (a) {
            case 0: {
              // ab cd ef
              // =>
              // n = abc + 0x11 = bc + 0x11
              // ofs = def + 1
              cd.SplitNibbles(out var c, out var d);
              var ef = br.ReadByte();

              n = ((b << 4) | c) + 0x11;
              ofs = ((d << 8) | ef) + 1;
              break;
            }
            case 1: {
              // ab cd ef gh
              // =>
              // n = bcde + 0x111
              // ofs = fgh + 1
              br.ReadByte().SplitNibbles(out var e, out var f);
              var gh = br.ReadByte();

              n = ((b << 12) | (cd << 4) | e) +
                  0x111;
              ofs = ((f << 8) | gh) + 1;
              break;
            }
            default: {
              // ab cd
              // =>
              // n = a + 1
              // ofs = bcd + 1
              n = a + 1;
              ofs = ((b << 8) | cd) + 1;
              break;
            }
          }

          if (data.Count + n > decompressedSize) {
            Asserts.Fail("Too much data!");
          }

          if (data.Count < ofs) {
            Asserts.Fail("Not enough data!");
          }

          for (var ii = 0; ii < n; ii++) {
            var x = data[data.Count - ofs];
            data.Add(x);
          }
        }

        if (data.Count >= decompressedSize) {
          break;
        }
      }
    }

    return data.ToArray();
  }

  private static uint ReadDecompressedSize_(IBinaryReader br) {
    var decompressedSize = br.ReadUInt24();
    if (decompressedSize == 0) {
      decompressedSize = br.ReadUInt32();
    }

    if (decompressedSize < 40) {
      Asserts.Fail($"LZ77 decompressed size is too small: {decompressedSize}");
    }

    if (decompressedSize > (1 << 19) * 4) {
      Asserts.Fail($"LZ77 decompressed size is too big: {decompressedSize}");
    }

    return decompressedSize;
  }
}