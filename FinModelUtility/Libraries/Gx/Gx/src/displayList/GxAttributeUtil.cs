using System.Numerics;

using fin.schema.color;
using fin.color;

using schema.binary;

namespace gx.displayList;

public static class GxAttributeUtil {
  public static int GetColorComponentCount(
      GxComponentCountType componentCountType)
    => GetComponentCount(GxVertexAttribute.Color0, componentCountType);

  public static int GetComponentCount(GxVertexAttribute vertexAttribute,
                                      GxComponentCountType componentCountType)
    => vertexAttribute switch {
        GxVertexAttribute.Position => componentCountType switch {
            GxComponentCountType.POS_XY  => 2,
            GxComponentCountType.POS_XYZ => 3,
            _                            => throw new ArgumentOutOfRangeException(nameof(componentCountType), componentCountType, null)
        },
        GxVertexAttribute.Normal => componentCountType switch {
            GxComponentCountType.NRM_XYZ => 3,
            _                            => throw new ArgumentOutOfRangeException(nameof(componentCountType), componentCountType, null)
        },
        GxVertexAttribute.NBT => componentCountType switch {
            GxComponentCountType.NRM_NBT => 3,
            _                            => throw new ArgumentOutOfRangeException(nameof(componentCountType), componentCountType, null)
        },
        GxVertexAttribute.Color0 or GxVertexAttribute.Color1
            => componentCountType switch {
                GxComponentCountType.CLR_RGB  => 3,
                GxComponentCountType.CLR_RGBA => 4,
                _                             => throw new ArgumentOutOfRangeException(nameof(componentCountType), componentCountType, null)
            },
        >= GxVertexAttribute.Tex0Coord and <= GxVertexAttribute.Tex7Coord
            => componentCountType switch {
                GxComponentCountType.TEX_S  => 1,
                GxComponentCountType.TEX_ST => 2,
                _                           => throw new ArgumentOutOfRangeException(nameof(componentCountType), componentCountType, null)
            },
        _ => throw new ArgumentOutOfRangeException(nameof(vertexAttribute), vertexAttribute, null)
    };

  public static int GetByteCount(this GxAxisComponentType axisComponentType)
    => axisComponentType switch {
        GxAxisComponentType.U8  => 1,
        GxAxisComponentType.S8  => 1,
        GxAxisComponentType.U16 => 2,
        GxAxisComponentType.S16 => 2,
        GxAxisComponentType.F32 => 4,
        _                       => throw new ArgumentOutOfRangeException(nameof(axisComponentType), axisComponentType, null)
    };

  public static float GetScale(byte decimalPoint)
    => MathF.Pow(0.5f, decimalPoint);

  public static float ReadValue(IBinaryReader br,
                                GxAxisComponentType axisComponentType,
                                byte decimalPoint)
    => GetScale(decimalPoint) * ReadValue(br, axisComponentType);

  public static float ReadValue(IBinaryReader br,
                                GxAxisComponentType axisComponentType)
    => axisComponentType switch {
        GxAxisComponentType.U8  => br.ReadByte(),
        GxAxisComponentType.S8  => br.ReadSByte(),
        GxAxisComponentType.U16 => br.ReadUInt16(),
        GxAxisComponentType.S16 => br.ReadInt16(),
        GxAxisComponentType.F32 => br.ReadSingle(),
        _                       => throw new ArgumentOutOfRangeException(nameof(axisComponentType), axisComponentType, null)
    };

  public static Rgba32 ReadColor(IBinaryReader br,
                                 GxColorComponentType colorComponentType) {
    switch (colorComponentType) {
      case GxColorComponentType.RGB565: {
        ColorUtil.SplitRgb565(br.ReadUInt16(),
                              out var r,
                              out var g,
                              out var b);
        return new Rgba32(r, g, b);
      }
      case GxColorComponentType.RGB8: {
        return new Rgba32(br.ReadByte(), br.ReadByte(), br.ReadByte());
      }
      case GxColorComponentType.RGBX8:
      case GxColorComponentType.RGBA8: {
        return new Rgba32(br.ReadByte(),
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
        return new Rgba32(r, g, b, a);
      }
      case GxColorComponentType.RGBA6: {
        var c = br.ReadUInt24();
        var r = (byte) ((((c >> 18) & 0x3F) << 2) | (((c >> 18) & 0x3F) >> 4));
        var g = (byte) ((((c >> 12) & 0x3F) << 2) | (((c >> 12) & 0x3F) >> 4));
        var b = (byte) ((((c >> 6) & 0x3F) << 2) | (((c >> 6) & 0x3F) >> 4));
        var a = (byte) (((c & 0x3F) << 2) | ((c & 0x3F) >> 4));
        return new Rgba32(r, g, b, a);
      }
      default: throw new ArgumentOutOfRangeException();
    }
  }

  public static Vector3 ReadPosition(IBinaryReader br,
                                     GxComponentCountType componentCountType,
                                     GxAxisComponentType axisComponentType,
                                     byte decimalPoint) {
    var componentCount = GxAttributeUtil.GetComponentCount(
        GxVertexAttribute.Position,
        componentCountType);

    Span<float> floats = stackalloc float[3];
    for (var i = 0; i < componentCount; i++) {
      floats[i] = ReadValue(br, axisComponentType, decimalPoint);
    }

    return new Vector3(floats);
  }

  public static Vector3 ReadNormal(IBinaryReader br,
                                   GxComponentCountType componentCountType,
                                   GxAxisComponentType axisComponentType,
                                   byte decimalPoint) {
    var componentCount = GxAttributeUtil.GetComponentCount(
        GxVertexAttribute.Normal,
        componentCountType);

    Span<float> floats = stackalloc float[3];
    for (var i = 0; i < componentCount; i++) {
      floats[i] = ReadValue(br, axisComponentType, decimalPoint);
    }

    return new Vector3(floats);
  }

  public static Vector2 ReadTexCoord(IBinaryReader br,
                                     GxComponentCountType componentCountType,
                                     GxAxisComponentType axisComponentType,
                                     byte decimalPoint) {
    var componentCount = GxAttributeUtil.GetComponentCount(
        GxVertexAttribute.Tex0Coord,
        componentCountType);

    Span<float> floats = stackalloc float[2];
    for (var i = 0; i < componentCount; i++) {
      floats[i] = ReadValue(br, axisComponentType, decimalPoint);
    }

    return new Vector2(floats);
  }
}