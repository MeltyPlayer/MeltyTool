using System.Collections;
using System.Collections.Generic;

namespace fin.data.dictionaries {
  /// <summary>
  ///   Dictionary that handles overwriting existing values by calling some
  ///   aggregator function.
  ///
  ///   Helpful for things like finding minimums/maximums.
  /// </summary>
  public class AggregatedDictionary<TKey, TValue>(
      AggregatedDictionary<TKey, TValue>.AggregatorFunction aggregateFunc,
      IFinDictionary<TKey, TValue> impl)
      : IFinDictionary<TKey, TValue> {
    public delegate TValue AggregatorFunction(
        TValue existingValue,
        TValue newValue);

    public AggregatedDictionary(AggregatorFunction aggregateFunc) : this(
        aggregateFunc,
        new SimpleDictionary<TKey, TValue>()) { }

    public void Clear() => impl.Clear();
    public int Count => impl.Count;
    public IEnumerable<TKey> Keys => impl.Keys;
    public IEnumerable<TValue> Values => impl.Values;
    public bool ContainsKey(TKey key) => impl.ContainsKey(key);

    public TValue this[TKey key] {
      get => impl[key];
      set {
        if (impl.TryGetValue(key, out var existingValue)) {
          impl[key] = aggregateFunc(existingValue, value);
        } else {
          impl[key] = value;
        }
      }
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