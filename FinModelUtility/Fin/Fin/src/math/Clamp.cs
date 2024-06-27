using System;
using System.Runtime.CompilerServices;

namespace fin.math;

public static class Clamp {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Byte(long value)
    => (byte) Math.Clamp(value, byte.MinValue, byte.MaxValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte Sbyte(long value)
    => (sbyte) Math.Clamp(value, sbyte.MinValue, sbyte.MaxValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort UInt16(long value)
    => (ushort) Math.Clamp(value, ushort.MinValue, ushort.MaxValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short Int16(long value)
    => (short) Math.Clamp(value, short.MinValue, short.MaxValue);
}