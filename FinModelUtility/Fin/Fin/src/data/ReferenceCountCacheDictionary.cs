using System;
using System.Collections.Generic;

namespace fin.data;

public interface IReferenceCountCacheDictionary<in TKey, out TValue> {
  public TValue GetAndIncrement(TKey key);
  public void DecrementAndMaybeDispose(TKey key);
}

public class ReferenceCountCacheDictionary<TKey, TValue>(
    Func<TKey, TValue> createHandler,
    Action<TKey, TValue>? disposeHandler = null)
    : IReferenceCountCacheDictionary<TKey, TValue>
    where TKey : notnull {
  private readonly Dictionary<TKey, (TValue value, int count)> impl_ = new();

  public TValue GetAndIncrement(TKey key) {
    if (this.impl_.TryGetValue(key, out var valueAndCount)) {
      ++valueAndCount.count;
      return valueAndCount.value;
    }

    var value = createHandler(key);
    this.impl_.Add(key, (value, 1));

    return value;
  }

  public void DecrementAndMaybeDispose(TKey key) {
    if (this.impl_.TryGetValue(key, out var valueAndCount)) {
      if (--valueAndCount.count <= 0) {
        this.impl_.Remove(key);
        disposeHandler?.Invoke(key, valueAndCount.value);
      }
    }
  }
}