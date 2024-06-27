using System;
using System.Collections;
using System.Collections.Generic;

using fin.data.dictionaries;

namespace fin.data.lazy;

/// <summary>
///   Dictionary implementation that lazily populates its entries when
///   accessed.
/// </summary>
public class LazyDictionary<TKey, TValue>
    : ILazyDictionary<TKey, TValue> {
  private readonly IFinDictionary<TKey, TValue> impl_;
  private readonly Func<TKey, TValue> handler_;

  public LazyDictionary(Func<TKey, TValue> handler, IFinDictionary<TKey, TValue>? impl = null) {
    this.handler_ = handler;
    this.impl_ = impl ?? new NullFriendlyDictionary<TKey, TValue>();
  }

  public LazyDictionary(Func<LazyDictionary<TKey, TValue>, TKey, TValue> handler, IFinDictionary<TKey, TValue>? impl = null) {
    this.handler_ = key => handler(this, key);
    this.impl_ = impl ?? new NullFriendlyDictionary<TKey, TValue>();
  }

  public int Count => this.impl_.Count;
  public void Clear() => this.impl_.Clear();
  public bool ContainsKey(TKey key) => this.impl_.ContainsKey(key);

  public bool Remove(TKey key) => this.impl_.Remove(key);

  public TValue this[TKey key] {
    get => this.impl_.TryGetValue(key, out var value)
        ? value
        : this.impl_[key] = this.handler_(key);
    set => this.impl_[key] = value;
  }

  public IEnumerable<TKey> Keys => this.impl_.Keys;
  public IEnumerable<TValue> Values => this.impl_.Values;

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(TKey Key, TValue Value)> GetEnumerator()
    => this.impl_.GetEnumerator();
}