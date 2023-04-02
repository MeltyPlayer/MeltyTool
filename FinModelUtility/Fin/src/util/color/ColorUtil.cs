using System;
using System.Runtime.CompilerServices;

using fin.color;
using fin.math;


namespace fin.util.color {
  public static class ColorUtil {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ExtractScaled(ushort col, int offset, int count) {
      var maxPossible = 1 << count;
      var factor = 255f / maxPossible;
      return ColorUtil.ExtractScaled(col, offset, count, factor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ExtractScaled(
        ushort col,
        int offset,
        int count,
        float factor) {
      var extracted = BitLogic.ExtractFromRight(col, offset, count);
      return (byte) (extracted * factor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SplitRgb565(
        ushort color,
        out byte r,
        out byte g,
        out byte b) {
      var upper = (byte) (color >> 8);
      var lower = (byte) (color);

      r = (byte) ((uint) upper & 248U);
      b = (byte) ((int) lower << 3 & 248);
      g = (byte) ((int) upper << 5 & 224 | (int) lower >> 3 & 28);
    }

    public static IColor ParseRgb565(ushort color) {
      ColorUtil.SplitRgb565(color, out var r, out var g, out var b);
      return FinColor.FromRgbBytes(r, g, b);
    }

    public static void SplitRgb5A3(
        ushort color,
        out byte r,
        out byte g,
        out byte b,
        out byte a) {
      var alphaFlag = BitLogic.ExtractFromRight(color, 15, 1);

      if (alphaFlag == 1) {
        a = 255;
        r = ColorUtil.ExtractScaled(color, 10, 5);
        g = ColorUtil.ExtractScaled(color, 5, 5);
        b = ColorUtil.ExtractScaled(color, 0, 5);
      } else {
        a = ColorUtil.ExtractScaled(color, 12, 3);
        r = ColorUtil.ExtractScaled(color, 8, 4, 17);
        g = ColorUtil.ExtractScaled(color, 4, 4, 17);
        b = ColorUtil.ExtractScaled(color, 0, 4, 17);
      }
    }

    public static IColor ParseRgb5A3(ushort color) {
      ColorUtil.SplitRgb5A3(color, out var r, out var g, out var b, out var a);
      return FinColor.FromRgbaBytes(r, g, b, a);
    }

    public static void SplitRgb5A1(
        ushort color,
        out byte r,
        out byte g,
        out byte b,
        out byte a) {
      var alphaFlag = BitLogic.ExtractFromRight(color, 15, 1);

      if (alphaFlag == 1) {
        a = 255;
        r = ColorUtil.ExtractScaled(color, 10, 5);
        g = ColorUtil.ExtractScaled(color, 5, 5);
        b = ColorUtil.ExtractScaled(color, 0, 5);
      } else {
        a = 0;
        r = ColorUtil.ExtractScaled(color, 10, 5);
        g = ColorUtil.ExtractScaled(color, 5, 5);
        b = ColorUtil.ExtractScaled(color, 0, 5);
      }
    }

    public static void SplitRgba4444(
        ushort color,
        out byte r,
        out byte g,
        out byte b,
        out byte a) {
      r = ColorUtil.ExtractScaled(color, 12, 4);
      g = ColorUtil.ExtractScaled(color, 8, 4);
      b = ColorUtil.ExtractScaled(color, 4, 4);
      a = ColorUtil.ExtractScaled(color, 0, 4);
    }

    public static IColor Interpolate(IColor from, IColor to, double amt) {
      ColorUtil.Interpolate(from.Rb,
                            from.Gb,
                            from.Bb,
                            from.Ab,
                            to.Rb,
                            to.Gb,
                            to.Bb,
                            to.Ab,
                            amt,
                            out var r,
                            out var g,
                            out var b,
                            out var a);
      return FinColor.FromRgbaBytes(r, g, b, a);
    }

    public static void Interpolate(
        byte fromR,
        byte fromG,
        byte fromB,
        byte fromA,
        byte toR,
        byte toG,
        byte toB,
        byte toA,
        double amt,
        out byte outR,
        out byte outG,
        out byte outB,
        out byte outA) {
      outR = (byte) Math.Round(fromR * (1 - amt) + toR * amt);
      outG = (byte) Math.Round(fromG * (1 - amt) + toG * amt);
      outB = (byte) Math.Round(fromB * (1 - amt) + toB * amt);
      outA = (byte) Math.Round(fromA * (1 - amt) + toA * amt);
    }
  }
}