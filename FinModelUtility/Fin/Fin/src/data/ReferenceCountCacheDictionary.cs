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
  private readonly ConcurrentDictionary<TKey, (TValue value, int count)> impl_
      = new();

  public TValue GetAndIncrement(TKey key) {
    var valueAndCount = this.impl_.GetOrAdd(key, _ => (createHandler(key), 0));
    valueAndCount.count++;

    if (valueAndCount.count == 1) {
      countChangedHandler?.Invoke(this.impl_.Count);
    }

    return valueAndCount.value;
  }

  public void DecrementAndMaybeDispose(TKey key) {
    if (this.impl_.TryGetValue(key, out var valueAndCount)) {
      if (--valueAndCount.count <= 0) {
        this.impl_.Remove(key, out _);
        disposeHandler?.Invoke(key, valueAndCount.value);

        countChangedHandler?.Invoke(this.impl_.Count);
      }
    } else {
      // TODO: Uh... what does this case mean???
      ;
    }
  }
}