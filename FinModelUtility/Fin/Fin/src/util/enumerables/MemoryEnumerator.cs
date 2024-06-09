using System.Collections.Generic;

namespace fin.util.enumerables {
  public interface IMemoryEnumerator<TValue> {
    TValue Current { get; }

    bool TryMoveNext() => this.TryMoveNext(out _);
    bool TryMoveNext(out TValue value);

    TValue TryMoveNextAndGetCurrent() {
      this.TryMoveNext();
      return this.Current;
    }
  }

  public class MemoryEnumerator<TEnumerated, TValue>
      : IMemoryEnumerator<TValue> {
    private readonly IEnumerator<TEnumerated> impl_;
    private readonly TryMoveNextDelegate tryMoveNextHandler_;

    public delegate bool TryMoveNextDelegate(
        IEnumerator<TEnumerated> enumerator,
        out TValue value);

    public MemoryEnumerator(
        IEnumerator<TEnumerated> impl,
        TryMoveNextDelegate tryMoveNextHandler) {
      this.impl_ = impl;
      this.tryMoveNextHandler_ = tryMoveNextHandler;
    }

    public TValue Current { get; private set; } = default;

    public bool TryMoveNext(out TValue nextValue) {
      if (!this.tryMoveNextHandler_(this.impl_, out nextValue)) {
        return false;
      }

      this.Current = nextValue;
      return true;
    }
  }

  public static class MemoryEnumeratorExtensions {
    public static IMemoryEnumerator<TValue>
        ToMemoryEnumerator<TEnumerated, TValue>(
            this IEnumerable<TEnumerated> enumerable,
            MemoryEnumerator<TEnumerated, TValue>.TryMoveNextDelegate
                tryMoveNextDelegate)
      => new MemoryEnumerator<TEnumerated, TValue>(
          enumerable.GetEnumerator(),
          tryMoveNextDelegate);

    public static IMemoryEnumerator<T> ToMemoryEnumerator<T>(
        this IEnumerable<T> enumerable)
      => new MemoryEnumerator<T, T>(
          enumerable.GetEnumerator(),
          EnumeratorExtensions.TryMoveNext);
  }
}