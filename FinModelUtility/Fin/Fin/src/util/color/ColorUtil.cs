using System;
using System.Runtime.CompilerServices;

using fin.color;
using fin.math;

namespace fin.util.color;

public static class ColorUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ExtractScaled(ushort col, int offset, int count) {
    var maxPossible = 1 << count;
    var factor = 255f / maxPossible;
    return ExtractScaled(col, offset, count, factor);
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
    r = (byte) ((color >> 8) & 0b11111000);
    g = (byte) ((color >> 3) & 0b11111100);
    b = (byte) (color << 3 & 0b11111000);
  }

  public static IColor ParseRgb565(ushort color) {
    SplitRgb565(color, out var r, out var g, out var b);
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
      r = ExtractScaled(color, 10, 5);
      g = ExtractScaled(color, 5, 5);
      b = ExtractScaled(color, 0, 5);
    } else {
      a = ExtractScaled(color, 12, 3);
      r = ExtractScaled(color, 8, 4, 17);
      g = ExtractScaled(color, 4, 4, 17);
      b = ExtractScaled(color, 0, 4, 17);
    }
  }

  public static IColor ParseRgb5A3(ushort color) {
    SplitRgb5A3(color, out var r, out var g, out var b, out var a);
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
      r = ExtractScaled(color, 10, 5);
      g = ExtractScaled(color, 5, 5);
      b = ExtractScaled(color, 0, 5);
    } else {
      a = 0;
      r = ExtractScaled(color, 10, 5);
      g = ExtractScaled(color, 5, 5);
      b = ExtractScaled(color, 0, 5);
    }
  }

  public static void SplitArgb1555(
      ushort color,
      out byte r,
      out byte g,
      out byte b,
      out byte a) {
    var alphaFlag = BitLogic.ExtractFromRight(color, 0, 1);

    if (alphaFlag == 1) {
      a = 255;
      r = ExtractScaled(color, 11, 5);
      g = ExtractScaled(color, 6, 5);
      b = ExtractScaled(color, 1, 5);
    } else {
      a = 0;
      r = ExtractScaled(color, 11, 5);
      g = ExtractScaled(color, 6, 5);
      b = ExtractScaled(color, 1, 5);
    }
  }

  public static void SplitRgba4444(
      ushort color,
      out byte r,
      out byte g,
      out byte b,
      out byte a) {
    r = ExtractScaled(color, 12, 4);
    g = ExtractScaled(color, 8, 4);
    b = ExtractScaled(color, 4, 4);
    a = ExtractScaled(color, 0, 4);
  }

  public static IColor Interpolate(IColor from, IColor to, double amt) {
    Interpolate(from.Rb,
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