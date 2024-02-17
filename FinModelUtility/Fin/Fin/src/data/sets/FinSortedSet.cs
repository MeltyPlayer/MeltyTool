using System;
using System.Collections;
using System.Collections.Generic;

namespace fin.data.sets {
  public class FinSortedSet<T>(Comparer<T> comparer) : IFinSet<T> {
    private readonly SortedSet<T> impl_ = new(comparer);

    public FinSortedSet() : this(Comparer<T>.Default) { }

    public FinSortedSet(Comparison<T> comparison) : this(
        Comparer<T>.Create(comparison)) { }

    public int Count => this.impl_.Count;

    public bool Contains(T value) => this.impl_.Contains(value);

    public void Clear() => this.impl_.Clear();

    public bool Add(T value) => this.impl_.Add(value);
    public bool Remove(T value) => this.impl_.Remove(value);

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    public IEnumerator<T> GetEnumerator() => this.impl_.GetEnumerator();
  }
}