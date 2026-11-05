using System;
using System.Runtime.CompilerServices;


namespace fin.math;

public static partial class BitLogic {
  private static readonly byte[] TEMP_ = new byte[4];

  public static void SplitNibbles(this byte value,
                                  out byte high,
                                  out byte low) {
    high = (byte) (value >> 4);
    low = (byte) (value & 0xF);
  }

  public static void SplitCrumbs(this byte value,
                                 out byte c0,
                                 out byte c1,
                                 out byte c2,
                                 out byte c3) {
    c0 = (byte) (value >> 6);
    c1 = (byte) ((value >> 4) & 0x3);
    c2 = (byte) ((value >> 2) & 0x3);
    c3 = (byte) (value & 0x3);
  }


  public static uint ToUint32(byte a, byte b, byte c, byte d) {
    TEMP_[0] = a;
    TEMP_[1] = b;
    TEMP_[2] = c;
    TEMP_[3] = d;

    return BitConverter.ToUInt32(TEMP_, 0);
  }

  public static (byte, byte, byte, byte) FromUint32(uint value) {
    BitConverter.TryWriteBytes(TEMP_, value);

    var a = TEMP_[0];
    var b = TEMP_[1];
    var c = TEMP_[2];
    var d = TEMP_[3];

    return (a, b, c, d);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint GetMask(int numBits) => (uint) ((1 << numBits) - 1);


  /// <summary>
  //    Function to extract k bits from p position and returns the extracted
  //    value as integer
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ExtractFromRight(this uint number, int offset, int count)
    => ((number >>> offset) & GetMask(count));

  /// <summary>
  //    Function to extract k bits from p position and returns the extracted
  //    value as integer
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort ExtractFromRight(this ushort number,
                                        int offset,
                                        int count)
    => (ushort) ((number >> offset) & GetMask(count));

  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint SetFromRight(this uint input,
                                  int offset,
                                  int count,
                                  uint value) {
    var mask = GetMask(count);
    value &= mask;

    var invertedMask = ~(mask << offset);

    return (input & invertedMask) | (value << offset);
  }

  public static double ConvertBinaryFractionToDouble(ushort binaryFraction)
    => 1f * binaryFraction / 0x10000;

  public static ushort ConvertDoubleToBinaryFraction(double doubleFraction)
    => (ushort) (doubleFraction * 0x10000).Clamp(
        ushort.MinValue,
        ushort.MaxValue);

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/magcius/noclip.website/blob/1d5ee0e1c4fd4f447456f0019b6d64dff2fc9bd1/src/Common/N64/Image.ts#L56-L58
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Expand3To8(int n)
    => (byte) ((n << (8 - 3)) | (n << (8 - 6)) | (n >>> (9 - 8)));

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/magcius/noclip.website/blob/1d5ee0e1c4fd4f447456f0019b6d64dff2fc9bd1/src/Common/N64/Image.ts#L60-L62
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Expand4To8(int n)
    => (byte) ((n << (8 - 4)) | (n >>> (8 - 8)));

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/magcius/noclip.website/blob/1d5ee0e1c4fd4f447456f0019b6d64dff2fc9bd1/src/Common/N64/Image.ts#L64-L66
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Expand5To8(int n)
    => (byte) ((n << (8 - 5)) | (n >>> (10 - 8)));
}