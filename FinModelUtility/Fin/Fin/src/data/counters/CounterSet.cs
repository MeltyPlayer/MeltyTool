using System.Collections.Generic;

namespace fin.data.counters;

public class CounterSet<T> {
  private readonly Dictionary<T, int> impl_ = new();

  public int Increment(T key) {
    if (this.impl_.TryGetValue(key, out var count)) {
      count++;
    }

    return this.impl_[key] = count;
  }

  public void Clear() => this.impl_.Clear();
  public int Count => this.impl_.Count;
}