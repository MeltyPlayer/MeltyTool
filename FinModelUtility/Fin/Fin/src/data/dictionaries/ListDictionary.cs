using System.Collections.Generic;
using System.Linq;

using schema.readOnly;

namespace fin.data.dictionaries;

[GenerateReadOnly]
public partial interface IListDictionary<TKey, TValue> {
  [Const]
  new bool HasList(TKey key);

  new IList<TValue> this[TKey key] { get; }

  void ClearList(TKey key);
  void Add(TKey key, TValue value);

  new IEnumerable<TKey> Keys { get; }
  new IEnumerable<TValue> Values { get; }
}

public static class ListDictionaryExtensions {
  public static bool TryGetList<TKey, TValue>(
      this IListDictionary<TKey, TValue> impl,
      TKey key,
      out IList<TValue> list) {
    if (impl.HasList(key)) {
      list = impl[key];
      return true;
    }

    list = default;
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

    list = default;
    return false;
  }

  public static IEnumerable<(TKey key, IList<TValue> value)> GetPairs<
      TKey, TValue>(this IListDictionary<TKey, TValue> impl)
    => impl.Keys.Select(key => (key, impl[key]));

  public static IEnumerable<(TKey key, IReadOnlyList<TValue> value)> GetPairs<
      TKey, TValue>(this IReadOnlyListDictionary<TKey, TValue> impl)
    => impl.Keys.Select(key => (key, impl[key]));
}


/// <summary>
///   An implementation for a dictionary of lists. Each value added for a key
///   will be stored in that key's corresponding list.
/// </summary>
public class ListDictionary<TKey, TValue>(
    IFinDictionary<TKey, IList<TValue>> impl)
    : IListDictionary<TKey, TValue> {
  public ListDictionary() : this(
      new NullFriendlyDictionary<TKey, IList<TValue>>()) { }

  public void Clear() => impl.Clear();
  public void ClearList(TKey key) => impl.Remove(key);

  public int Count => impl.Values.Select(list => list.Count).Sum();

  public bool HasList(TKey key) => impl.ContainsKey(key);

  public void Add(TKey key, TValue value) {
    IList<TValue> list;
    if (!impl.TryGetValue(key, out list)) {
      impl[key] = list = new List<TValue>();
    }

    list.Add(value);
  }

  public IList<TValue> this[TKey key] => impl[key];

  public IEnumerable<TKey> Keys => impl.Keys;
  public IEnumerable<TValue> Values => impl.Values.SelectMany(v => v);
}