using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

using fin.data.dictionaries;

namespace fin.data.lazy {
  public class LazyCaseInvariantStringDictionary<TValue> : ILazyDictionary<string, TValue> {
    private readonly ILazyDictionary<string, TValue> impl_;

    public LazyCaseInvariantStringDictionary(Func<string, TValue> handler) {
      this.impl_ = new LazyDictionary<string, TValue>(
          handler,
          new SimpleDictionary<string, TValue>(new ConcurrentDictionary<string, TValue>(StringComparer.OrdinalIgnoreCase)));
    }

    public LazyCaseInvariantStringDictionary(Func<LazyDictionary<string, TValue>, string, TValue> handler) {
      this.impl_ = new LazyDictionary<string, TValue>(
          handler,
          new SimpleDictionary<string, TValue>(new ConcurrentDictionary<string, TValue>(StringComparer.OrdinalIgnoreCase)));
    }

    public void Clear() => this.impl_.Clear();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    public IEnumerator<(string Key, TValue Value)> GetEnumerator() => this.impl_.GetEnumerator();

    public int Count => this.impl_.Count;
    public IEnumerable<string> Keys => this.impl_.Keys;
    public IEnumerable<TValue> Values => this.impl_.Values;

    public bool ContainsKey(string key) => this.impl_.ContainsKey(key);

    public bool Remove(string key) => this.impl_.Remove(key);

    public TValue this[string key] {
      get => this.impl_[key];
      set => this.impl_[key] = value;
    }
  }
}