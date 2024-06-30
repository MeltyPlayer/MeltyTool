using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.util.asserts;

namespace fin.util.enumerables;

public static class EnumerableExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<T> Nonnull<T>(this IEnumerable<T?> enumerable)
    => enumerable.Where(value => value != null)!;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int IndexOf<T>(this IEnumerable<T> enumerable,
                               T value) {
    var index = enumerable.IndexOfOrNegativeOne(value);
    Asserts.True(index > -1);
    return index;
  }

  public static int IndexOfOrNegativeOne<T>(
      this IEnumerable<T> enumerable,
      T value) {
    var index = 0;
    foreach (var item in enumerable) {
      if (value?.Equals(item) ?? (value == null && item == null)) {
        return index;
      }

      index++;
    }

    return -1;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<T> ConcatIfNonnull<T>(
      this IEnumerable<T> enumerable,
      IEnumerable<T>? other)
    => other == null ? enumerable : enumerable.Concat(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<T> ConcatIfNonnull<T>(
      this IEnumerable<T> enumerable,
      T? other)
    => other == null ? enumerable : enumerable.Concat(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<T> Yield<T>(this T item) {
    yield return item;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable,
                                         T item)
    => enumerable.Concat(item.Yield());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static List<T> AsList<T>(this T item)
    => item.Yield().ToList();


  public static IEnumerable<(T X, T Y)> SeparatePairs<T>(
      this IEnumerable<T> enumerable) {
    using var iterator = enumerable.GetEnumerator();
    while (iterator.MoveNext()) {
      var v1 = iterator.Current;

      iterator.MoveNext();
      var v2 = iterator.Current;

      yield return (v1, v2);
    }
  }

  public static IEnumerable<(T X, T Y, T Z)> SeparateTriplets<T>(
      this IEnumerable<T> enumerable) {
    using var iterator = enumerable.GetEnumerator();
    while (iterator.MoveNext()) {
      var v1 = iterator.Current;

      iterator.MoveNext();
      var v2 = iterator.Current;

      iterator.MoveNext();
      var v3 = iterator.Current;

      yield return (v1, v2, v3);
    }
  }

  public static IEnumerable<(T X, T Y, T Z, T W)> SeparateQuadruplets<T>(
      this IEnumerable<T> enumerable) {
    using var iterator = enumerable.GetEnumerator();
    while (iterator.MoveNext()) {
      var v1 = iterator.Current;

      iterator.MoveNext();
      var v2 = iterator.Current;

      iterator.MoveNext();
      var v3 = iterator.Current;

      iterator.MoveNext();
      var v4 = iterator.Current;

      yield return (v1, v2, v3, v4);
    }
  }


  public static bool SequenceEqualOrBothEmpty<T>(
      this IEnumerable<T>? lhs,
      IEnumerable<T>? rhs) {
    if (lhs == null && rhs == null) {
      return true;
    }

    return (lhs ?? []).SequenceEqual(rhs ?? []);
  }
}