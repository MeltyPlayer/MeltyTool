using System.Collections.Generic;

namespace fin.data.indexable;

public class IndexedSetDictionary<TValue> {
  private readonly IndexedDictionary<HashSet<TValue>> impl_ = new();

  public void Clear() => this.impl_.Clear();

  public ISet<TValue> GetOrCreateSet(int index) {
    if (!this.impl_.TryGetValue(index, out var set)) {
      this.impl_[index] = set = new HashSet<TValue>();
    }

    return set;
  }


  public void Add(int index, TValue value)
    => this.GetOrCreateSet(index).Add(value);

  public ISet<TValue> this[int index] => this.impl_[index];

  public bool TryGetSet(int index, out IReadOnlySet<TValue> set)
    => this.impl_.TryGetValue(index, out set);
}