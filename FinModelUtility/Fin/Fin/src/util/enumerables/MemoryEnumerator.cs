using System;
using System.Collections.Generic;

namespace fin.util.enumerables;

public interface IMemoryEnumerator<TValue> {
  TValue Current { get; }

  bool TryMoveNext() => this.TryMoveNext(out _);
  bool TryMoveNext(out TValue value);

  TValue TryMoveNextAndGetCurrent() {
    this.TryMoveNext();
    return this.Current;
  }

  bool TryReadInto(Span<TValue> dst) {
    var didRead = false;
    for (var i = 0; i < dst.Length; ++i) {
      if (this.TryMoveNext()) {
        didRead = true;
      }

      dst[i] = this.Current;
    }

    return didRead;
  }
}

public class MemoryEnumerator<T>(
    IEnumerator<T> impl,
    MemoryEnumerator<T, T>.TryMoveNextDelegate tryMoveNextHandler)
    : MemoryEnumerator<T, T>(impl, tryMoveNextHandler);

public class MemoryEnumerator<TEnumerated, TValue>(
    IEnumerator<TEnumerated> impl,
    MemoryEnumerator<TEnumerated, TValue>.TryMoveNextDelegate
        tryMoveNextHandler)
    : IMemoryEnumerator<TValue> {
  public delegate bool TryMoveNextDelegate(
      IEnumerator<TEnumerated> enumerator,
      out TValue value);

  public TValue Current { get; private set; } = default;

  public bool TryMoveNext(out TValue nextValue) {
    if (!tryMoveNextHandler(impl, out nextValue)) {
      return false;
    }

    this.Current = nextValue;
    return true;
  }
}

public static class MemoryEnumeratorExtensions {
  public static IMemoryEnumerator<T> ToMemoryEnumerator<T>(
      this IEnumerable<T> enumerable)
    => new MemoryEnumerator<T>(
        enumerable.GetEnumerator(),
        EnumeratorExtensions.TryMoveNext);
}