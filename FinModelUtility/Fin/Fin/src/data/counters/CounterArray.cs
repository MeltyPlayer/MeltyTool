using System.Collections;
using System.Collections.Generic;

using fin.data.lists;

namespace fin.data.counters;

public class CounterArray : IFinList<int> {
  private readonly List<int> impl_ = [];

  public int Count => this.impl_.Count;

  public void Clear() => this.impl_.Clear();

  public int Increment(int index) {
    while (this.impl_.Count < index + 1) {
      this.impl_.Add(0);
    }

    return this.impl_[index] += 1;
  }

  public int this[int index] {
    get => this.impl_[index];
    set => this.impl_[index] = value;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<int> GetEnumerator() => this.impl_.GetEnumerator();
}