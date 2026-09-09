using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using readOnly;

namespace fin.data.indexable;

[GenerateReadOnly]
public partial interface IIndexableDictionary<in TIndexable, TValue>
    : IEnumerable<TValue>
    where TIndexable : IIndexable {
  new int Count { get; }

  // Have to specify only contains key because "out" method parameters
  // aren't allowed to be covariant:
  // https://github.com/dotnet/csharplang/discussions/5623
  [Const]
  new bool ContainsKey(int index);

  [Const]
  new bool ContainsKey(TIndexable key);

  void Clear();
  new TValue this[int index] { get; set; }
  new TValue this[TIndexable key] { get; set; }

  bool Remove(TIndexable key);
}

public static class IndexableDictionaryExtensions {
  public static bool TryGetValue<TKey, TValue>(
      this IReadOnlyIndexableDictionary<TKey, TValue> impl,
      TKey key,
      out TValue value) where TKey : IIndexable
    => impl.TryGetValue(key.Index, out value);

  public static bool TryGetValue<TKey, TValue>(
      this IReadOnlyIndexableDictionary<TKey, TValue> impl,
      int index,
      out TValue value) where TKey : IIndexable {
    if (impl.ContainsKey(index)) {
      value = impl[index];
      return true;
    }

    value = default!;
    return false;
  }
}

public sealed class IndexableDictionary<TIndexable, TValue>(int capacity)
    : IIndexableDictionary<TIndexable, TValue>
    where TIndexable : IIndexable {
  private readonly List<(bool hasValue, TValue value)> impl_ = new(capacity);

  public IndexableDictionary() : this(0) { }

  public int Count { get; private set; }

  public void Clear() {
    this.impl_.Clear();
    this.impl_.EnsureCapacity(capacity);
  }

  public TValue this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_[index].Item2;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      this.impl_.EnsureCapacity(index);

      while (this.impl_.Count <= index) {
        this.impl_.Add((false, default));
      }

      if (!this.impl_[index].hasValue) {
        ++this.Count;
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

  public bool ContainsKey(TIndexable key) => this.ContainsKey(key.Index);

  public bool ContainsKey(int index) {
    if (index >= this.impl_.Count) {
      return false;
    }

    return this.impl_[index].hasValue;
  }

  public bool Remove(TIndexable key) {
    if (!this.ContainsKey(key)) {
      return false;
    }

    --this.Count;
    this.impl_[key.Index] = (false, default!);
    return true;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<TValue> GetEnumerator()
    => this.impl_.Where(pair => pair.hasValue)
           .Select(pair => pair.value)
           .GetEnumerator();
}