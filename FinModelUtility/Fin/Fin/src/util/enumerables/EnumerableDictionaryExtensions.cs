using System;
using System.Collections.Generic;
using System.Linq;

using fin.data.indexable;

namespace fin.util.enumerables;

public static class EnumerableDictionaryExtensions {
  public static ISparseIndexableDictionary<T, int>
      ToIndexByValueSparseIndexableDictionary<T>(
          this IEnumerable<T> enumerable) where T : IIndexable
    => enumerable.Select((value, index) => (value, index))
                 .ToSparseIndexableDictionary();

  public static ISparseIndexableDictionary<TKey, TValue> ToSparseIndexableDictionary<
      TKey, TValue>(this IEnumerable<(TKey key, TValue value)> enumerable)
      where TKey : IIndexable
    => enumerable.ToSparseIndexableDictionary(pair => pair.key,
                                        pair => pair.value);

  public static ISparseIndexableDictionary<TKey, TValue> ToSparseIndexableDictionary<
      T, TKey, TValue>(
      this IEnumerable<T> enumerable,
      Func<T, TKey> keySelector,
      Func<T, TValue> valueSelector)
      where TKey : IIndexable {
    var SparseIndexableDictionary = new SparseIndexableDictionary<TKey, TValue>();
    foreach (var t in enumerable) {
      SparseIndexableDictionary[keySelector(t)] = valueSelector(t);
    }

    return SparseIndexableDictionary;
  }
}