using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.pixel;
using fin.math;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/master/packages/lib/models/FORM.ts#L540
/// </summary>
[BinarySchema]
public sealed partial class Bmp1Data : IFormChunkData {
  [RSequenceUntilEndOfStream]
  public byte[] Data { get; set; }

  public IImage ToImage(IDictionary<ushort, Pal1Data> pal1ByGlobalIndex) {
    using var bmpBr = new SchemaBinaryReader(this.Data, Endianness.BigEndian);

    var unk0 = bmpBr.ReadUInt16();
    var format = bmpBr.ReadUInt16();
    var unk1 = bmpBr.ReadByte();
    var width = bmpBr.ReadUInt16();
    var height = bmpBr.ReadUInt16();

    switch (format) {
      case 0x128 or 0x228: {
        // Traditional bitmap format
        // 0x128 is just a bitmap
        // 0x228 starts out the same, but TODO it also has some sort of extra RGBA data appended.
        //   mp1 0/93, 0/94, 0/95, 46/2, 48/15, 54/5, 57/3

        var paletteGlobalIndex = bmpBr.ReadUInt16();
        var pal1 = pal1ByGlobalIndex[paletteGlobalIndex];

        bmpBr.Position = 0xf;
        var inBmpSize = bmpBr.ReadUInt16();

        // Doesn't appear to indicate BPP, but we can calculate from size and dimens.
        var inBpp = 8 / ((width * height) / inBmpSize);
        var inBmpData = bmpBr.ReadBytes(inBmpSize);

        var indexImage = new I8Image(inBpp switch {
                                         2 => PixelFormat.P2,
                                         4 => PixelFormat.P4,
                                         8 => PixelFormat.P8,
                                     },
                                     width,
                                     height);

        {
          using var fastLock = indexImage.Lock();
          var scan0 = fastLock.Bytes;

          var outIndex = 0;
          var inIdx = 0;

          while (inIdx < inBmpData.Length) {
            if (inBpp == 8) {
              scan0[outIndex++] = inBmpData[inIdx++];
            } else if (inBpp == 4) {
              inBmpData[inIdx++].SplitNibbles(out var upper, out var lower);

              scan0[outIndex++] = upper;
              scan0[outIndex++] = lower;
            } else if (inBpp == 2) {
              inBmpData[inIdx++]
                  .SplitCrumbs(out var c0, out var c1, out var c2, out var c3);

              scan0[outIndex++] = c0;
              scan0[outIndex++] = c1;
              scan0[outIndex++] = c2;
              scan0[outIndex++] = c3;
            }
          }
        }

        return new IndexedImage8(pal1.Format, indexImage, pal1.Colors);
      }
      case 0x127: {
        // mp1 9/138
        // Just raw RGBA already
        bmpBr.Position = 0x4;
        var bpp = bmpBr.ReadByte();

        bmpBr.Position = 0xd;
        return bpp switch {
            16 => PixelImageReader
                  .New(width, height, new Rgba5551PixelReader())
                  .ReadImage(bmpBr),
            32 => PixelImageReader
                  .New(width, height, new Rgba32PixelReader())
                  .ReadImage(bmpBr),
            _ => throw new ArgumentOutOfRangeException()
        };
      }
      case 0x126: {
        // 0x126 mp1 9/25
        // 24 bits per color
        // TODO: Are the bits right, or is there alpha?
        bmpBr.Position = 0xd;
        return PixelImageReader
               .New(width, height, new Rgb24PixelReader())
               .ReadImage(bmpBr);
      }
      case 0x125: {
        // Grayscale?
        // TODO: I don't think this is right? Some sort of shadow mask?
        bmpBr.Position = 0xd;
        return PixelImageReader
               .New(width, height, new La16PixelReader())
               .ReadImage(bmpBr);
      }
      case 0x124: {
        // 0x124 mp3 11/51
        // Grayscale?
        // TODO: I don't think this is right? Some sort of shadow mask?
        bmpBr.Position = 0xd;
        return PixelImageReader
               .New(width, height, new L4PixelReader())
               .ReadImage(bmpBr);
      }
    }

    // TODO: Other formats

    throw new NotImplementedException();
  }
}