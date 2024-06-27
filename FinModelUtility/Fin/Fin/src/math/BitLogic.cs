using System;
using System.Runtime.CompilerServices;

namespace fin.math;

public static class BitLogic {
  private static readonly byte[] TEMP_ = new byte[4];

  public static uint ToUint32(byte a, byte b, byte c, byte d) {
    BitLogic.TEMP_[0] = a;
    BitLogic.TEMP_[1] = b;
    BitLogic.TEMP_[2] = c;
    BitLogic.TEMP_[3] = d;

    return BitConverter.ToUInt32(BitLogic.TEMP_, 0);
  }

  public static (byte, byte, byte, byte) FromUint32(uint value) {
    BitConverter.TryWriteBytes(BitLogic.TEMP_, value);

    var a = BitLogic.TEMP_[0];
    var b = BitLogic.TEMP_[1];
    var c = BitLogic.TEMP_[2];
    var d = BitLogic.TEMP_[3];

    return (a, b, c, d);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint GetMask(int numBits) => (uint) ((1 << numBits) - 1);


  /// <summary>
  //    Function to extract k bits from p position
  //    and returns the extracted value as integer
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ExtractFromRight(this uint number, int offset, int count)
      // => (((1 << count) - 1) & (number >> (offset - 1)));
    => ((number >> offset) & GetMask(count));


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this byte number, int offset)
    => ((number >> offset) & 1) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this ushort number, int offset)
    => ((number >> offset) & 1) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this uint number, int offset)
    => ((number >> offset) & 1) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this ulong number, int offset)
    => ((number >> offset) & 1) != 0;


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this sbyte number, int offset)
    => ((number >> offset) & 1) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this short number, int offset)
    => ((number >> offset) & 1) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this int number, int offset)
    => ((number >> offset) & 1) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this long number, int offset)
    => ((number >> offset) & 1) != 0;

  public static double ConvertBinaryFractionToDouble(ushort binaryFraction)
    => binaryFraction / (ushort.MaxValue + 1.0);
}