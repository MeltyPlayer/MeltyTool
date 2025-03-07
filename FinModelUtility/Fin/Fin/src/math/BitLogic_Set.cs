using System.Runtime.CompilerServices;

namespace fin.math;

public static partial class BitLogic {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte SetBitTo1(this byte number, int offset)
    => (byte) (number | (1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort SetBitTo1(this ushort number, int offset)
    => (ushort) (number | (1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint SetBitTo1(this uint number, int offset)
    => number | (uint) (1 << offset);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong SetBitTo1(this ulong number, int offset)
    => number | (ulong) (1 << offset);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte SetBitTo1(this sbyte number, int offset)
    => (sbyte) (number | (1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short SetBitTo1(this short number, int offset)
    => (short) (number | (1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int SetBitTo1(this int number, int offset)
    => number | (1 << offset);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long SetBitTo1(this long number, int offset)
    => number | (1 << offset);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte SetBitTo0(this byte number, int offset)
    => (byte) (number & ~(1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort SetBitTo0(this ushort number, int offset)
    => (ushort) (number & ~(1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint SetBitTo0(this uint number, int offset)
    => number & ~((uint) (1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong SetBitTo0(this ulong number, int offset)
    => number & ~((ulong) (1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte SetBitTo0(this sbyte number, int offset)
    => (sbyte) (number & ~(1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short SetBitTo0(this short number, int offset)
    => (short) (number & ~(1 << offset));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int SetBitTo0(this int number, int offset)
    => number & ~(1 << offset);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long SetBitTo0(this long number, int offset)
    => number & ~(1 << offset);
}