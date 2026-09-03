using System;
using System.Collections.Generic;
using System.Linq;

using fin.util.linq;

namespace fin.data.dictionaries;

public static class ListDictionaryExtensions {
  public static TValue? GetSingleOrDefault<TKey, TValue>(
      this IListDictionary<TKey, TValue> impl,
      TKey key) {
    if (impl.TryGetList(key, out var list) && list.TryGetSingle(out var value)) {
      return value;
    }

    return default;
  }

  public static bool TryGetList<TKey, TValue>(
      this IListDictionary<TKey, TValue> impl,
      TKey key,
      out IList<TValue> list) {
    if (impl.HasList(key)) {
      list = impl[key];
      return true;
    }

    list = null;
    return false;
  }

  public static bool TryGetList<TKey, TValue>(
      this IReadOnlyListDictionary<TKey, TValue> impl,
      TKey key,
      out IReadOnlyList<TValue> list) {
    if (impl.HasList(key)) {
      list = impl[key];
      return true;
    }

    list = null!;
    return false;
  }

  public static IEnumerable<(TKey key, IList<TValue> value)> GetPairs<
      TKey, TValue>(this IListDictionary<TKey, TValue> impl)
    => impl.Keys.Select(key => (key, impl[key]));

  public static IEnumerable<(TKey key, IReadOnlyList<TValue> value)> GetPairs<
      TKey, TValue>(this IReadOnlyListDictionary<TKey, TValue> impl)
    => impl.Keys.Select(key => (key, impl[key]));

  public static IListDictionary<TKey, T> ToListDictionary<T, TKey>(
      this IEnumerable<T> enumerable,
      Func<T, TKey> keySelector)
    => enumerable.ToListDictionary(keySelector, v => v);

  public static IListDictionary<TKey, TValue> ToListDictionary<T, TKey, TValue>(
      this IEnumerable<T> enumerable,
      Func<T, TKey> keySelector,
      Func<T, TValue> valueSelector) {
    var listDictionary = new ListDictionary<TKey, TValue>();
    foreach (var value in enumerable) {
      listDictionary.Add(keySelector(value), valueSelector(value));
    }

    return listDictionary;
  }
}