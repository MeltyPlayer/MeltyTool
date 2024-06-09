using System;
using System.Collections.Generic;
using System.Linq;

using fin.data.indexable;

namespace fin.util.enumerables {
  public static class EnumerableDictionaryExtensions {
    public static IIndexableDictionary<T, int>
        ToIndexByValueIndexableDictionary<T>(
            this IEnumerable<T> enumerable) where T : IIndexable
      => enumerable.Select((value, index) => (value, index))
                   .ToIndexableDictionary();

    public static IIndexableDictionary<TKey, TValue> ToIndexableDictionary<
        TKey, TValue>(this IEnumerable<(TKey key, TValue value)> enumerable)
        where TKey : IIndexable
      => enumerable.ToIndexableDictionary(pair => pair.key,
                                          pair => pair.value);

    public static IIndexableDictionary<TKey, TValue> ToIndexableDictionary<
        T, TKey, TValue>(
        this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector)
        where TKey : IIndexable {
      var indexableDictionary = new IndexableDictionary<TKey, TValue>();
      foreach (var t in enumerable) {
        indexableDictionary[keySelector(t)] = valueSelector(t);
      }

      return indexableDictionary;
    }
  }
}