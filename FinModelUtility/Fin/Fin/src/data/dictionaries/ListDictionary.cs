using System.Collections;
using System.Collections.Generic;
using System.Linq;

using schema.readOnly;

namespace fin.data.dictionaries {
  [GenerateReadOnly]
  public partial interface IListDictionary<TKey, TValue>
      : IFinCollection<(TKey Key, IList<TValue> Value)> {
    [Const]
    bool HasList(TKey key);

    IList<TValue> this[TKey key] { get; }

    [Const]
    bool TryGetList(TKey key, out IList<TValue>? list);

    void ClearList(TKey key);
    void Add(TKey key, TValue value);
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

    public bool TryGetList(TKey key, out IList<TValue>? list)
      => impl.TryGetValue(key, out list);

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<(TKey Key, IList<TValue> Value)> GetEnumerator()
      => impl.GetEnumerator();
  }
}