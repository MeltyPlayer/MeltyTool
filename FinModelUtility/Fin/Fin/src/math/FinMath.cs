using System;
using System.Numerics;
using System.Runtime.CompilerServices;


namespace fin.math;

public static class FinMath {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInRange<TNumber>(this TNumber value,
                                        TNumber min,
                                        TNumber max)
      where TNumber : INumber<TNumber>
    => min <= value && value <= max;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TNumber MapRange<TNumber>(this TNumber value,
                                          TNumber minFrom,
                                          TNumber maxFrom,
                                          TNumber minTo,
                                          TNumber maxTo)
      where TNumber : INumber<TNumber>
    => minTo + (value - minFrom) / (maxFrom - minFrom) * (maxTo - minTo);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TNumber Clamp<TNumber>(this TNumber value,
                                       TNumber min,
                                       TNumber max)
      where TNumber : INumber<TNumber>
    => TNumber.Max(min, TNumber.Min(value, max));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TNumber Wrap<TNumber>(this TNumber value,
                                      TNumber min,
                                      TNumber max)
      where TNumber : INumber<TNumber> {
    if (min == max) {
      return min;
    }

    if (value < min) {
      return max - ((min - value) % (max - min));
    }

    if (value > max) {
      return min + ((value - min) % (max - min));
    }

    return value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TNumber ModRange<TNumber>(this TNumber value,
                                          TNumber min,
                                          TNumber max)
      where TNumber : INumber<TNumber> {
    if (value < min) {
      return max - ((min - value) % (max - min));
    }

    return min + ((value - min) % (max - min));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float RoundToNearest(this float value, float target)
    => MathF.Round(value / target) * target;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CeilToNearest(this float value, float target)
    => MathF.Ceiling(value / target) * target;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int CeilToNearest(this int value, int target)
    => (int) MathF.Ceiling(1f * value / target) * target;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float FloorToNearest(this float value, float target)
    => MathF.Floor(value / target) * target;

  public static int Base10DigitCount(this int value)
    => (int) Math.Max(1, 1 + Math.Log10(value));

  public static bool IsTruthy<TNumber>(this TNumber? number)
      where TNumber : struct, INumber<TNumber>
    => (number ?? default) != default;


  public static TNumber Min<TNumber>(params ReadOnlySpan<TNumber> numbers)
      where TNumber : struct, INumber<TNumber> {
    var min = numbers[0];
    for (var i = 1; i < numbers.Length; ++i) {
      min = TNumber.Min(min, numbers[i]);
    }

    return min;
  }

  public static TNumber Max<TNumber>(params ReadOnlySpan<TNumber> numbers)
      where TNumber : struct, INumber<TNumber> {
    var max = numbers[0];
    for (var i = 1; i < numbers.Length; ++i) {
      max = TNumber.Max(max, numbers[i]);
    }

    return max;
  }
}