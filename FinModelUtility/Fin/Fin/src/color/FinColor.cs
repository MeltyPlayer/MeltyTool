using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.color;

public interface IColor {
  float Rf { get; }
  float Gf { get; }
  float Bf { get; }
  float Af { get; }

  byte Rb { get; }
  byte Gb { get; }
  byte Bb { get; }
  byte Ab { get; }
}

public class FinColor : IColor {
  private FinColor(byte rb, byte gb, byte bb, byte ab) {
    this.Rb = rb;
    this.Gb = gb;
    this.Bb = bb;
    this.Ab = ab;
  }

  public static IColor FromRgbBytes(byte rb, byte gb, byte bb)
    => FromRgbaBytes(rb, gb, bb, 255);

  public static IColor FromRgba(Rgba32 rgba)
    => new FinColor(rgba.R, rgba.G, rgba.B, rgba.A);

  public static IColor FromRgbaBytes(byte rb, byte gb, byte bb, byte ab)
    => new FinColor(rb, gb, bb, ab);


  public static IColor FromRgbFloats(float rf, float gf, float bf)
    => FromRgbaFloats(rf, gf, bf, 1);

  public static IColor FromRgbaFloats(float rf, float gf, float bf, float af)
    => FromRgbaBytes((byte) (rf * 255),
                     (byte) (gf * 255),
                     (byte) (bf * 255),
                     (byte) (af * 255));


  public static IColor FromIntensityByte(byte ib)
    => FromRgbBytes(ib, ib, ib);

  public static IColor FromIntensityFloat(float iF)
    => FromIntensityByte((byte) (iF * 255));


  public static IColor FromAlphaFloat(float af)
    => FromRgbaFloats(1, 1, 1, af);


  public static IColor FromSystemColor(Color color)
    => FromRgbaBytes(color.R, color.G, color.B, color.A);

  public static Color ToSystemColor<TColor>(TColor color)
      where TColor : IColor
    => Color.FromArgb(color.Ab, color.Rb, color.Gb, color.Bb);

  public static IColor Lerp(IColor from, IColor to, float frac) {
    var r = (byte) Math.Sqrt(Lerp_(from.Rb * from.Rb, to.Rb * to.Rb, frac));
    var g = (byte) Math.Sqrt(Lerp_(from.Gb * from.Gb, to.Gb * to.Gb, frac));
    var b = (byte) Math.Sqrt(Lerp_(from.Bb * from.Bb, to.Bb * to.Bb, frac));

    return FinColor.FromRgbBytes(r, g, b);
  }

  private static float Lerp_(float from, float to, float frac)
    => from * (1 - frac) + to * frac;

  public float Rf => this.Rb / 255f;
  public float Gf => this.Gb / 255f;
  public float Bf => this.Bb / 255f;
  public float Af => this.Ab / 255f;

  public byte Rb { get; }
  public byte Gb { get; }
  public byte Bb { get; }
  public byte Ab { get; }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SplitBgra(int bgra,
                               out byte r,
                               out byte g,
                               out byte b,
                               out byte a) {
    b = (byte) bgra;
    g = (byte) (bgra >> 8);
    r = (byte) (bgra >> 16);
    a = (byte) (bgra >> 24);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SplitRgba(int rgba,
                               out byte r,
                               out byte g,
                               out byte b,
                               out byte a) {
    r = (byte) rgba;
    g = (byte) (rgba >> 8);
    b = (byte) (rgba >> 16);
    a = (byte) (rgba >> 24);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SplitRgb(int rgb, out byte r, out byte g, out byte b) {
    r = (byte) rgb;
    g = (byte) (rgb >> 8);
    b = (byte) (rgb >> 16);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MergeBgra(byte r, byte g, byte b, byte a)
    => b | (g << 8) | (r << 16) | (a << 24);
}