using System.Runtime.CompilerServices;

namespace fin.math;

public static partial class BitLogic {
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
}