using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using schema.readOnly;

namespace fin.data.indexable;

[GenerateReadOnly]
public partial interface IIndexableDictionary<in TIndexable, TValue>
    : IEnumerable<TValue>
    where TIndexable : IIndexable {
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
}

public static class IndexableDictionaryExtensions {
  public static bool TryGetValue<TKey, TValue>(
      this IReadOnlyIndexableDictionary<TKey, TValue> impl,
      TKey key,
      out TValue value) where TKey : IIndexable {
    if (impl.ContainsKey(key)) {
      value = impl[key];
      return true;
    }

    value = default;
    return false;
  }
}

public class IndexableDictionary<TIndexable, TValue>(int capacity)
    : IIndexableDictionary<TIndexable, TValue>
    where TIndexable : IIndexable {
  private readonly List<(bool hasValue, TValue value)> impl_ = new(capacity);

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

  public bool ContainsKey(TIndexable key) => this.ContainsKey(key.Index);

  public bool ContainsKey(int index) {
    if (index >= this.impl_.Count) {
      return false;
    }

    return this.impl_[index].hasValue;
  }


  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<TValue> GetEnumerator()
    => this.impl_.Where(pair => pair.hasValue)
           .Select(pair => pair.value)
           .GetEnumerator();
}