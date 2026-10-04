using System.Drawing;

using fin.color;
using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.pixel;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using schema.binary;

namespace berserkDc.api;

public sealed record PacModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle<PacModelFileBundle, PacModelImporter>;

public sealed class PacModelImporter
    : IModelImporter<PacModelImporter, PacModelFileBundle> {
  public IModel Import(PacModelFileBundle fileBundle) {
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.MainFile.AsFileSet()
    };

    using var br
        = fileBundle.MainFile.OpenReadAsBinary(Endianness.LittleEndian);

    var chunkCount = br.ReadUInt32();
    var chunkOffsets = br.ReadUInt32s(chunkCount);

    foreach (var chunkOffset in chunkOffsets) {
      br.Position = chunkOffset;

      var magic = br.ReadString(4);
      if (magic == "NJCM") {
        var njcmSize = br.ReadUInt32();
      } else {
        br.Position -= 4;
        br.PushLocalSpace();

        var textureCount = br.ReadUInt32();
        var textureOffsets = br.ReadUInt32s(textureCount);

        foreach (var textureOffset in textureOffsets) {
          br.Position = textureOffset;
          
          var img = ReadImage_(br);

          finModel.MaterialManager.CreateTexture(img);
        }

        br.PopLocalSpace();
      }
    }

    return finModel;
  }

  private static IImage ReadImage_(IBinaryReader br) {
    var unk0 = br.ReadUInt32();
    var unk1 = br.ReadUInt32();
    var width = br.ReadUInt32();
    var height = br.ReadUInt32();

    var imageType = (ImageType) ((unk1 & 0xFF00) >> 8);
    if (imageType.HasPalette(width, height, out var paletteSize)) {
      var palette = br.ReadUInt16s(paletteSize)
                      .Select(ColorUtil.ParseRgb565)
                      .ToArray();

      var img = PixelImageReader.New((int) width / 2, (int) height / 2, new L8PixelReader())
                                .ReadImage(br);

      return new IndexedImage8(PixelFormat.RGB565, img, palette);
    }

    return FinImage.Create1x1FromColor(Color.Magenta);
  }
}

public enum ImageType {
  TYPE_0x01 = 1,
  TYPE_0x02,
  TYPE_0x03,
  TYPE_0x04,
  TYPE_0x05,
  TYPE_0x07 = 0x7,
  TYPE_0x08,
  TYPE_0x0D = 0xd,
  TYPE_0x10 = 0x10,
  TYPE_0x11,
  TYPE_0x12,
}

public static class ImageTypeExtensions {
  extension(ImageType imageType) {
    public bool HasPalette(uint width, uint height, out int paletteSize) {
      if (!(imageType is ImageType.TYPE_0x03
                         or ImageType.TYPE_0x04
                         or ImageType.TYPE_0x10
                         or ImageType.TYPE_0x11)) {
        paletteSize = 0;
        return false;
      }

      if (imageType is ImageType.TYPE_0x03 or ImageType.TYPE_0x04) {
        paletteSize = 0x100;
        return true;
      }

      paletteSize =
          (int) (
              ((int) (height * width + (height * width >> 0x1f & 0x1fU)) >> 5) +
              0xfU &
              0xfffffff0);
      if ((imageType == ImageType.TYPE_0x11) &&
          (0x1f < (int) paletteSize)) {
        paletteSize = paletteSize * 2;
      }

      return true;
    }
  }
}