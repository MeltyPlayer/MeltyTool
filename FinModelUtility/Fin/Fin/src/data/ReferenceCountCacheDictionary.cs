using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace fin.data;

public interface IReferenceCountCacheDictionary<in TKey, out TValue> {
  TValue GetAndIncrement(TKey key);
  void DecrementAndMaybeDispose(TKey key);
}

public sealed class ReferenceCountCacheDictionary<TKey, TValue>(
    Func<TKey, TValue> createHandler,
    Action<TKey, TValue>? disposeHandler = null,
    Action<int>? countChangedHandler = null)
    : IReferenceCountCacheDictionary<TKey, TValue>
    where TKey : notnull {
  private readonly ConcurrentDictionary<TKey, ValueAndCount> impl_
      = new();

  public TValue GetAndIncrement(TKey key) {
    var valueAndCount
        = this.impl_.GetOrAdd(key, _ => new(createHandler(key), 0));
    valueAndCount.Count++;

    if (valueAndCount.Count == 1) {
      countChangedHandler?.Invoke(this.impl_.Count);
    }

    return valueAndCount.Value;
  }

  public void DecrementAndMaybeDispose(TKey key) {
    if (this.impl_.TryGetValue(key, out var valueAndCount)) {
      if (--valueAndCount.Count <= 0) {
        this.impl_.Remove(key, out _);
        disposeHandler?.Invoke(key, valueAndCount.Value);

        countChangedHandler?.Invoke(this.impl_.Count);
      }
    } else {
      // TODO: This can happen because the extra renderers for single meshes
      // clean up shaders redundantly.
    }
  }

  private record ValueAndCount(TValue Value, int Count) {
    public int Count { get; set; } = Count;
  }
}