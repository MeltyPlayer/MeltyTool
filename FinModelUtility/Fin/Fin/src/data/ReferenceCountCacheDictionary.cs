using System;
using System.Collections.Generic;

namespace fin.data;

public interface IReferenceCountCacheDictionary<in TKey, out TValue> {
  public TValue GetAndIncrement(TKey key);
  public void DecrementAndMaybeDispose(TKey key);
}

public class ReferenceCountCacheDictionary<TKey, TValue> :
    IReferenceCountCacheDictionary<TKey, TValue> where TKey : notnull {
  private readonly Dictionary<TKey, (TValue value, int count)> impl_ = new();
  private readonly Func<TKey, TValue> createHandler_;
  private readonly Action<TKey, TValue>? disposeHandler_;

  public ReferenceCountCacheDictionary(Func<TKey, TValue> createHandler,
                                       Action<TKey, TValue>? disposeHandler = null) {
    this.createHandler_ = createHandler;
    this.disposeHandler_ = disposeHandler;
  }

  public TValue GetAndIncrement(TKey key) {
    if (this.impl_.TryGetValue(key, out var valueAndCount)) {
      ++valueAndCount.count;
      return valueAndCount.value;
    }

    var value = this.createHandler_(key);
    this.impl_.Add(key, (value, 1));

    return value;
  }

  public void DecrementAndMaybeDispose(TKey key) {
    if (this.impl_.TryGetValue(key, out var valueAndCount)) {
      if (--valueAndCount.count <= 0) {
        this.impl_.Remove(key);
        this.disposeHandler_?.Invoke(key, valueAndCount.value);
      }
    }
  }
}