using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace fin.data.indexable;

public sealed class IndexableDictionaryV2<TIndexable, TValue>(int capacity)
    : IIndexableDictionary<TIndexable, TValue>
    where TIndexable : IIndexable {
  private readonly List<bool> hasValueImpl_ = new(capacity);
  private readonly List<TValue> valueImpl_ = new(capacity);

  public IndexableDictionaryV2() : this(0) { }

  public int Count { get; private set; }

  public void Clear() {
    this.hasValueImpl_.Clear();
    this.hasValueImpl_.EnsureCapacity(capacity);

    this.valueImpl_.Clear();
    this.valueImpl_.EnsureCapacity(capacity);
  }

  public TValue this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.valueImpl_[index];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      this.hasValueImpl_.EnsureCapacity(index);
      this.valueImpl_.EnsureCapacity(index);

      while (this.valueImpl_.Count <= index) {
        this.hasValueImpl_.Add(false);
        this.valueImpl_.Add(default!);
      }

      if (!this.hasValueImpl_[index]) {
        ++this.Count;
      }

      this.hasValueImpl_[index] = true;
      this.valueImpl_[index] = value;
    }
  }

  public TValue this[TIndexable key] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this[key.Index];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this[key.Index] = value;
  }

  public bool ContainsKey(TIndexable key) => this.ContainsKey(key.Index);

  public bool ContainsKey(int index) {
    if (index >= this.hasValueImpl_.Count) {
      return false;
    }

    return this.hasValueImpl_[index];
  }

  public bool Remove(TIndexable key) {
    var index = key.Index;
    if (!this.ContainsKey(index)) {
      return false;
    }

    --this.Count;
    this.hasValueImpl_[index] = false;
    this.valueImpl_[index] = default!;
    return true;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<TValue> GetEnumerator() {
    for (var i = 0; i < this.hasValueImpl_.Count; ++i) {
      if (this.hasValueImpl_[i]) {
        yield return this.valueImpl_[i];
      }
    }
  }
}