using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using schema.readOnly;

namespace fin.data.indexable {
  [GenerateReadOnly]
  public partial interface IIndexableDictionary<TIndexable, TValue>
      : IEnumerable<TValue>
      where TIndexable : IIndexable {
    [Const]
    bool TryGetValue(int index, out TValue value);

    [Const]
    bool TryGetValue(TIndexable key, out TValue value);


    void Clear();
    TValue this[int index] { get; set; }
    TValue this[TIndexable key] { get; set; }
  }

  public class IndexableDictionary<TIndexable, TValue>(int capacity)
      : IIndexableDictionary<TIndexable, TValue>
      where TIndexable : IIndexable {
    private readonly List<(bool, TValue)> impl_ = new(capacity);

    public IndexableDictionary() : this(0) { }

    public void Clear() => this.impl_.Clear();

    public TValue this[int index] {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.impl_[index].Item2;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set {
        this.impl_.EnsureCapacity(index);

        while (this.impl_.Count <= index) {
          this.impl_.Add((false, default));
        }

        this.impl_[index] = (true, value);
      }
    }

    public TValue this[TIndexable key] {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this[key.Index];
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => this[key.Index] = value;
    }

    public bool TryGetValue(int index, out TValue value) {
      if (index >= this.impl_.Count) {
        value = default!;
        return false;
      }

      (var hasValue, value) = this.impl_[index];
      return hasValue;
    }

    public bool TryGetValue(TIndexable key, out TValue value)
      => this.TryGetValue(key.Index, out value);


    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<TValue> GetEnumerator()
      => this.impl_.Where(pair => pair.Item1)
             .Select(pair => pair.Item2)
             .GetEnumerator();
  }
}