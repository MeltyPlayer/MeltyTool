using System.Collections;
using System.Collections.Generic;

namespace fin.data.lists;

public partial class BinarySortedList<T>(
    IComparer<T> comparer,
    int capacity = 0) : IFinList<T> {
  private readonly List<T> impl_ = new(capacity);

  public void Clear() => this.impl_.Clear();
  public int Count => this.impl_.Count;

  public T this[int index] {
    get => this.impl_[index];
    set => this.impl_[index] = value;
  }

  public void Add(T value) {
    var index = this.impl_.BinarySearch(value, comparer);
    if (index < 0) {
      index = ~index;
    }

    this.impl_.Insert(index, value);
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<T> GetEnumerator() => this.impl_.GetEnumerator();
}