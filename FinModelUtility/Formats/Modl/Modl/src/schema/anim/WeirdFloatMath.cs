using System.Runtime.CompilerServices;

namespace modl.schema.anim;

public static class WeirdFloatMath {
  /// <summary> 0x0229C4AB </summary>
  public const float C_ZERO = 0;

  /// <summary> 0x38000000 </summary>
  public const float C_3_05175_EN5 = 3.0517578125E-05f;

  /// <summary> 0x38800000 </summary>
  public const float C_6_10351_EN5 = 6.103515625E-05f;

  /// <summary> 0x3F000000 </summary>
  public const float C_HALF = 0.5f;

  /// <summary> 0x40400000 </summary>
  public const float C_3 = 3;

  /// <summary> 0x46800000 </summary>
  public const float C_16384 = 16384f;

  /// <summary> 0x4330000000000000 </summary>
  public const double C_4503599627370496 = 4503599627370496;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Concat44(uint first, uint second)
    => ((ulong) first << 32) | second;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe double InterpretAsDouble(ulong value)
    => *(double*) (&value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe float InterpretAsSingle(uint value)
    => *(float*) (&value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double CreateWeirdDoubleFromUInt32(uint value)
    => WeirdFloatMath.InterpretAsDouble(
           WeirdFloatMath.Concat44(0x43300000, value)) -
       C_4503599627370496;
}