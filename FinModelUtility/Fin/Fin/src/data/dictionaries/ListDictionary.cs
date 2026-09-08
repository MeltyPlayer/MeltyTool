using System.Collections.Generic;
using System.Linq;

using readOnly;

namespace fin.data.dictionaries;

[GenerateReadOnly]
public partial interface IListDictionary<TKey, TValue> {
  int TotalCount { get; }

  [Const]
  new bool HasList(TKey key);

  new List<TValue> this[TKey key] { get; }

  void ClearList(TKey key);
  void Add(TKey key, TValue value);
  void AddRange(TKey key, IEnumerable<TValue> value);

  new IEnumerable<TKey> Keys { get; }
  new IEnumerable<TValue> Values { get; }
}

/// <summary>
///   An implementation for a dictionary of lists. Each value added for a key
///   will be stored in that key's corresponding list.
/// </summary>
public sealed class ListDictionary<TKey, TValue>(
    IFinDictionary<TKey, List<TValue>> impl)
    : IListDictionary<TKey, TValue> {
  public ListDictionary() : this(
      new NullFriendlyDictionary<TKey, List<TValue>>()) { }

  public int TotalCount => impl.Values.Select(list => list.Count).Sum();

  public void Clear() => impl.Clear();
  public void ClearList(TKey key) => impl.Remove(key);

  public bool HasList(TKey key) => impl.ContainsKey(key);

  public void Add(TKey key, TValue value)
    => impl.GetOrAdd(key, _ => []).Add(value);

  public void AddRange(TKey key, IEnumerable<TValue> values)
    => impl.GetOrAdd(key, _ => []).AddRange(values);

  public List<TValue> this[TKey key] => impl[key];

  public IEnumerable<TKey> Keys => impl.Keys;
  public IEnumerable<TValue> Values => impl.Values.SelectMany(v => v);
}