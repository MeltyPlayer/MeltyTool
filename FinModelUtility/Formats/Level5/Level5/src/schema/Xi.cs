using System.Drawing;

using FastBitmapLib;

using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.tile;
using fin.io;

using level5.decompression;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace level5.schema;

public class Xi {
  public int Width { get; set; }
  public int Height { get; set; }

  List<int> Tiles { get; set; } = [];

  public byte ImageFormat { get; set; }

  public byte[] ImageData { get; set; }

  private bool SwitchFile { get; set; } = false;

  public void Open(IReadOnlyGenericFile xiFile) {
    using var r = xiFile.OpenReadAsBinary(Endianness.LittleEndian);
    r.Position = 0x10;
    this.Width = r.ReadInt16();
    this.Height = r.ReadInt16();

    r.Position = 0xA;
    int type = r.ReadByte();

    r.Position = 0x1C;
    int someTable = r.ReadInt16();

    r.Position = 0x38;
    int someTableSize = r.ReadInt32();

    int imageDataOffset = someTable + someTableSize;

    var level5Decompressor = new Level5Decompressor();
    byte[] tileBytes =
        level5Decompressor.Decompress(
            r.SubreadAt((uint) someTable, ser => ser.ReadBytes(someTableSize)));

    if (tileBytes.Length > 2 && tileBytes[0] == 0x53 &&
        tileBytes[1] == 0x04)
      this.SwitchFile = true;

    using (var tileData =
           new SchemaBinaryReader(tileBytes, Endianness.LittleEndian)) {
      int tileCount = 0;
      while (tileData.Position + 2 <= tileData.Length) {
        int i = this.SwitchFile ? tileData.ReadInt32() : tileData.ReadInt16();
        if (i > tileCount) tileCount = i;
        this.Tiles.Add(i);
      }
    }

    switch (type) {
      case 0x1:
        type = 0x4;
        break;
      case 0x3:
        type = 0x1;
        break;
      case 0x4:
        type = 0x3;
        break;
      case 0x1B:
        type = 0xC;
        break;
      case 0x1C:
        type = 0xD;
        break;
      case 0x1D:
      case 0x1F:
        break;
      default:
        //File.WriteAllBytes("texture.bin", Decompress.Level5Decom(r.GetSection((uint)imageDataOffset, (int)(r.BaseStream.Length - imageDataOffset))));
        throw new Exception("Unknown Texture Type " + type.ToString("x"));
      //break;
    }

    this.ImageFormat = (byte) type;

    this.ImageData = level5Decompressor.Decompress(
        r.SubreadAt((uint) imageDataOffset,
                    ser => ser.ReadBytes((int) (r.Length - imageDataOffset))));
  }
    
  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public unsafe IImage ToBitmap() {
      Bitmap tileSheet;

      var imageFormat = (_3dsImageTools.TexFormat) this.ImageFormat;
      if (imageFormat is _3dsImageTools.TexFormat.ETC1
                         or _3dsImageTools.TexFormat.ETC1a4) {
        tileSheet = TiledImageReader.New(this.Tiles.Count * 8,
                                         8,
                                         new Etc1TileReader(
                                             imageFormat is _3dsImageTools.TexFormat.ETC1a4))
                                    .ReadImage(this.ImageData)
                                    .AsBitmap();
      } else {
        tileSheet = _3dsImageTools.DecodeImage(this.ImageData,
                                               this.Tiles.Count * 8,
            8,
            imageFormat);
      }

      var pixelFormat = imageFormat switch {
          _3dsImageTools.TexFormat.RGBA8    => PixelFormat.RGBA8888,
          _3dsImageTools.TexFormat.RGB8     => PixelFormat.RGB888,
          _3dsImageTools.TexFormat.RGBA5551 => PixelFormat.RGBA5551,
          _3dsImageTools.TexFormat.RGB565   => PixelFormat.RGB565,
          _3dsImageTools.TexFormat.RGBA4444 => PixelFormat.RGBA4444,
          _3dsImageTools.TexFormat.LA8      => PixelFormat.LA88,
          _3dsImageTools.TexFormat.HILO8    => PixelFormat.HILO88,
          _3dsImageTools.TexFormat.L8       => PixelFormat.L8,
          _3dsImageTools.TexFormat.A8       => PixelFormat.A8,
          _3dsImageTools.TexFormat.LA4      => PixelFormat.LA44,
          _3dsImageTools.TexFormat.L4       => PixelFormat.L4,
          _3dsImageTools.TexFormat.A4       => PixelFormat.A4,
          _3dsImageTools.TexFormat.ETC1     => PixelFormat.ETC1,
          _3dsImageTools.TexFormat.ETC1a4   => PixelFormat.ETC1A,
          _                                 => throw new ArgumentOutOfRangeException()
      };

      var tileSheetWidth = tileSheet.Width;

      var img = new Rgba32Image(pixelFormat, this.Width, this.Height);

      using var inputBmpData = tileSheet.FastLock();
      var inputPtr = (byte*) inputBmpData.Scan0;

      using var dstImgLock = img.Lock();
      var dstPtr = dstImgLock.Pixels;

      int y = 0;
      int x = 0;
      for (int i = 0; i < this.Tiles.Count; i++) {
        int code = this.Tiles[i];

        if (code != -1) {
          for (int h = 0; h < 8; h++) {
            for (int w = 0; w < 8; w++) {
              var inputIndex = 4 * ((code * 8 + w) + (h) * tileSheetWidth);
              var b = inputPtr[inputIndex];
              var g = inputPtr[inputIndex + 1];
              var r = inputPtr[inputIndex + 2];
              var a = inputPtr[inputIndex + 3];

              dstPtr[(x + w) * this.Width + y + h] = new Rgba32(r, g, b, a);
            }
          }
        }

        if (code == -1 && (this.ImageFormat == 0xC || this.ImageFormat == 0xD)) {
          for (int h = 0; h < 8; h++) {
            for (int w = 0; w < 8; w++) {
              dstPtr[(x + w) * this.Width + y + h] = new Rgba32(0, 0, 0, 0);
            }
          }
        }

        y += 8;

        if (y >= this.Width) {
          y = 0;
          x += 8;

          // TODO: This skips early, may not use all of the tiles. Is this right?
          if (x >= this.Height) {
            break;
          }
        }
      }

      return img;
    }
}