using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using schema.readOnly;

namespace fin.data.indexable;

[GenerateReadOnly]
public partial interface IIndexedDictionary<TValue>
    : IEnumerable<(int, TValue)> {
  [Const]
  new bool ContainsKey(int index);

  void Clear();
  new TValue this[int index] { get; set; }

  IEnumerable<TValue> Values { get; }
}

public static class IndexedDictionaryExtensions {
  public static bool TryGetValue<TValue>(
      this IReadOnlyIndexedDictionary<TValue> impl,
      int index,
      out TValue value) {
    if (impl.ContainsKey(index)) {
      value = impl[index];
      return true;
    }

    value = default!;
    return false;
  }
}

public class IndexedDictionary<TValue>(int capacity)
    : IIndexedDictionary<TValue> {
  private readonly List<(bool hasValue, TValue value)> impl_ = new(capacity);

  public IndexedDictionary() : this(0) { }

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

  public bool ContainsKey(int index) {
    if (index >= this.impl_.Count) {
      return false;
    }

    return this.impl_[index].hasValue;
  }


  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(int, TValue)> GetEnumerator()
    => this
       .impl_.Index()
       .Where(pairAndIndex => pairAndIndex.Item.hasValue)
       .Select(pairAndIndex => (pairAndIndex.Index, pairAndIndex.Item.value))
       .GetEnumerator();

  public IEnumerable<TValue> Values => this.impl_.Where(pair => pair.hasValue)
                                           .Select(pair => pair.value);
}