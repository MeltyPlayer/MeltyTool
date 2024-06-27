using System.Collections;
using System.Collections.Generic;
using System.Linq;

using fin.data.sets;

namespace fin.data.dictionaries;

/// <summary>
///   An implementation for a dictionary of ordered sets. Each value added for
///   a key will be stored in that key's corresponding sorted set. Sets will
///   remember the order that elements were added in, and enumerate in that
///   order.
/// </summary>
public class OrderedSetDictionary<TKey, TValue>
    : IFinCollection<(TKey Key, OrderedHashSet<TValue> Value)> {
  private readonly NullFriendlyDictionary<TKey, OrderedHashSet<TValue>> impl_
      = new();

  public void Clear() => this.impl_.Clear();
  public void ClearSet(TKey key) => this.impl_.Remove(key);

  public int Count => this.impl_.Values.Select(list => list.Count).Sum();

  public bool HasSet(TKey key) => this.impl_.ContainsKey(key);

  public void Add(TKey key, TValue value) {
    OrderedHashSet<TValue> set;
    if (!this.impl_.TryGetValue(key, out set)) {
      this.impl_[key] = set = [];
    }

    set.Add(value);
  }

  public OrderedHashSet<TValue> this[TKey key] => this.impl_[key];

  public bool TryGetSet(TKey key, out OrderedHashSet<TValue> set)
    => this.impl_.TryGetValue(key, out set);

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(TKey Key, OrderedHashSet<TValue> Value)> GetEnumerator()
    => this.impl_.GetEnumerator();
}