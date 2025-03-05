using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.util.enumerables;

public static class NumberEnumerableExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TNumber MaxOrDefault<T, TNumber>(
      this IEnumerable<T> enumerable, 
      Func<T, TNumber> selector,
      TNumber defaultValue = default)
      where TNumber : INumber<TNumber>
    => enumerable.Select(selector).MaxOrDefault(defaultValue);

  public static TNumber MaxOrDefault<TNumber>(
      this IEnumerable<TNumber> enumerable,
      TNumber defaultValue = default)
      where TNumber : INumber<TNumber> {
    var max = defaultValue;
    foreach (var value in enumerable) {
      max = TNumber.Max(max, value);
    }
    return max;
  }
}