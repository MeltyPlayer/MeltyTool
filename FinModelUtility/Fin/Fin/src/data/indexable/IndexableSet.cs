using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.data.sets;

using schema.readOnly;

namespace fin.data.indexable;

[GenerateReadOnly]
public partial interface IIndexableSet<TIndexable> : IFinSet<TIndexable>
    where TIndexable : IIndexable {
  [Const]
  new bool Contains(int index);
}

public class IndexableSet<TIndexable>(int capacity) : IIndexableSet<TIndexable>
    where TIndexable : IIndexable {
  private readonly List<(bool hasValue, TIndexable value)>
      impl_ = new(capacity);

  public IndexableSet() : this(0) { }

  public int Count => this.impl_.Count;
  public void Clear() => this.impl_.Clear();

  public bool Contains(TIndexable value) => this.Contains(value.Index);

  public bool Contains(int index)
    => index < this.Count && this.impl_[index].hasValue;

  public bool Add(TIndexable value) {
    var index = value.Index;
    this.impl_.EnsureCapacity(index);

    while (this.impl_.Count <= index) {
      this.impl_.Add((false, default));
    }

    var existed = this.impl_[index].hasValue;
    this.impl_[index] = (true, value);

    return !existed;
  }

  public bool Remove(TIndexable value) {
    var index = value.Index;
    if (value.Index >= this.Count) {
      return false;
    }

    var existed = this.impl_[index].hasValue;
    this.impl_[index] = (false, default);

    return existed;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<TIndexable> GetEnumerator()
    => this.impl_.Where(b => b.hasValue).Select(b => b.value).GetEnumerator();
}