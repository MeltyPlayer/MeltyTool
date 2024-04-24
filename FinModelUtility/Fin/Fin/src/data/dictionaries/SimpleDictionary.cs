using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace fin.data.dictionaries {
  public class SimpleDictionary<TKey, TValue>(IDictionary<TKey, TValue> impl)
      : IFinDictionary<TKey, TValue> {
    public SimpleDictionary() :
        this(new ConcurrentDictionary<TKey, TValue>()) { }

    public void Clear() => impl.Clear();

    public int Count => impl.Count;
    public IEnumerable<TKey> Keys => impl.Keys;
    public IEnumerable<TValue> Values => impl.Values;
    public bool ContainsKey(TKey key) => impl.ContainsKey(key);

    public TValue this[TKey key] {
      get => impl[key];
      set => impl[key] = value;
    }

    public bool Remove(TKey key) => impl.Remove(key);

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<(TKey Key, TValue Value)> GetEnumerator() {
      foreach (var (key, value) in impl) {
        yield return (key, value);
      }
    }
  }
}