using System.Collections.Generic;
using System.Linq;

namespace fin.util.dictionaries;

public static class DictionaryExtensions {
  public static IDictionary<TValue, TKey> SwapKeysAndValues<TKey, TValue>(
      this IReadOnlyDictionary<TKey, TValue> impl) where TValue : notnull
    => impl.ToDictionary(p => p.Value, p => p.Key);

  public static IEnumerable<T> Chain<T>(this IReadOnlyDictionary<T, T> impl,
                                        T first) {
    var current = first;
    do {
      yield return current;
    } while (impl.TryGetValue(current, out current));
  }
}