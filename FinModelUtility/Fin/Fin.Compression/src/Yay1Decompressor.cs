using schema.binary;

namespace fin.compression;

internal class Yay1Decompressor {
  private static byte[] DecompressYay1_(IBinaryReader br, out long compressedSize) {
    var baseOffset = br.Position;

    br.AssertString("Yay1");

    var decompressedSize = br.ReadUInt32();
    var dst = new byte[decompressedSize];
    var dstI = 0;

    var puVar5 = baseOffset + br.ReadUInt32();
    var pbVar9 = baseOffset + br.ReadUInt32();

    int in_t0_lo = 0;
    var iVar4 = 0;
    do {
      if (iVar4 == 0) {
        in_t0_lo = br.ReadInt32();
        iVar4 = 0x20;
      }
      if (in_t0_lo < 0) {
        dst[dstI++] = br.SubreadAt(pbVar9++, () => br.ReadByte());
      }
      else {
        br.SubreadAt(
            puVar5,
            () => {
              var uVar2 = br.ReadUInt16();

              var uVar7 = (uint)(uVar2 >> 0xc);
              var iVar6 = dstI - (uVar2 & 0xfff);

              int iVar8 = 0;
              if (uVar7 == 0) {
                br.SubreadAt(
                    pbVar9++,
                    () => {
                      var bVar1 = br.ReadByte();
                      iVar8 = bVar1 + 0x12;
                    });
              }
              else {
                iVar8 = (int) (uVar7 + 2);
              }
              do {
                var srcByte = dst[iVar6 + -1];
                --iVar8;
                ++iVar6;
                dst[dstI++] = srcByte;
              } while (iVar8 != 0);
            });

        puVar5 += 2;
      }

      in_t0_lo <<= 1;
      iVar4 -= 1;
    } while (dstI < dst.Length);

    compressedSize = br.Position - baseOffset - 0x10;

    return dst;
  }
}
