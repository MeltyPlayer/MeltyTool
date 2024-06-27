using System.Collections;
using System.Collections.Generic;

namespace fin.data.sets;

public class OrderedHashSet<T> : IFinSet<T> {
  // TODO: Can this be optimized??
  private readonly LinkedList<T> list_ = [];
  private readonly HashSet<T> set_ = [];

  public int Count => this.list_.Count;

  public bool Contains(T value) => this.set_.Contains(value);

  public void Clear() {
    this.list_.Clear();
    this.set_.Clear();
  }

  public bool Add(T value) {
    if (this.set_.Add(value)) {
      this.list_.AddLast(value);
      return true;
    }

    return false;
  }

  public bool Remove(T value) {
    if (this.set_.Remove(value)) {
      this.list_.Remove(value);
      return true;
    }

    return false;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<T> GetEnumerator() => this.list_.GetEnumerator();
}