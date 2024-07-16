using System.Collections;
using System.Collections.Generic;

namespace fin.data.counters;

public class CounterSet<T> : IEnumerable<KeyValuePair<T, int>> {
  private readonly Dictionary<T, int> impl_ = new();

  public int GetCount(T key) => this.impl_.GetValueOrDefault(key, 0);

  public int Increment(T key) {
    if (this.impl_.TryGetValue(key, out var count)) {
      this.impl_[key] = ++count;
    } else {
      count = this.impl_[key] = 1;
    }

    return count;
  }

  public int Decrement(T key) {
    if (this.impl_.TryGetValue(key, out var count)) {
      this.impl_[key] = --count;
    } else {
      count = this.impl_[key] = -1;
    }

    return count;
  }

  public void Clear() => this.impl_.Clear();
  public int Count => this.impl_.Count;

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<KeyValuePair<T, int>> GetEnumerator()
    => this.impl_.GetEnumerator();
}