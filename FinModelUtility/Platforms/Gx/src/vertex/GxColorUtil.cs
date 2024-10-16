using fin.color;
using fin.util.color;

using schema.binary;

namespace gx.vertex;

public static class GxColorUtil {
  public static IColor ReadColor(IBinaryReader br,
                                 GxColorComponentType colorComponentType) {
    switch (colorComponentType) {
      case GxColorComponentType.RGB565: {
        ColorUtil.SplitRgb565(br.ReadUInt16(),
                              out var r,
                              out var g,
                              out var b);
        return FinColor.FromRgbBytes(r, g, b);
      }
      case GxColorComponentType.RGB8: {
        return FinColor.FromRgbBytes(br.ReadByte(),
                                     br.ReadByte(),
                                     br.ReadByte());
      }
      case GxColorComponentType.RGBX8:
      case GxColorComponentType.RGBA8: {
        return FinColor.FromRgbaBytes(br.ReadByte(),
                                      br.ReadByte(),
                                      br.ReadByte(),
                                      br.ReadByte());
      }
      case GxColorComponentType.RGBA4: {
        ColorUtil.SplitRgba4444(
            br.ReadUInt16(),
            out var r,
            out var g,
            out var b,
            out var a);
        return FinColor.FromRgbaBytes(r, g, b, a);
      }
      case GxColorComponentType.RGBA6: {
        var c = br.ReadUInt24();
        var r = ((((c >> 18) & 0x3F) << 2) |
                 (((c >> 18) & 0x3F) >> 4)) /
                (float) 0xFF;
        var g = ((((c >> 12) & 0x3F) << 2) |
                 (((c >> 12) & 0x3F) >> 4)) /
                (float) 0xFF;
        var b = ((((c >> 6) & 0x3F) << 2) |
                 (((c >> 6) & 0x3F) >> 4)) /
                (float) 0xFF;
        var a = ((((c) & 0x3F) << 2) | (((c) & 0x3F) >> 4)) /
                (float) 0xFF;
        return FinColor.FromRgbaFloats(r, g, b, a);
      }
      default: throw new ArgumentOutOfRangeException();
    }
  }
}