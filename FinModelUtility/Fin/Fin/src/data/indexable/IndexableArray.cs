using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using readOnly;

namespace fin.data.indexable;

[GenerateReadOnly]
public partial interface IIndexableArray<in TIndexable, TValue>
    : IEnumerable<TValue>
    where TIndexable : IIndexable {
  new int Count { get; }

  new TValue this[int index] { get; set; }
  new TValue this[TIndexable key] { get; set; }
}

public sealed class IndexableArray<TIndexable, TValue>(int capacity)
    : IIndexableArray<TIndexable, TValue>
    where TIndexable : IIndexable {
  private readonly TValue[] impl_ = new TValue[capacity];

  public int Count => this.impl_.Length;

  public TValue this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_[index];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.impl_[index] = value;
  }

  public TValue this[TIndexable key] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this[key.Index];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this[key.Index] = value;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<TValue> GetEnumerator()
    => ((IEnumerable<TValue>) this.impl_).GetEnumerator();
}