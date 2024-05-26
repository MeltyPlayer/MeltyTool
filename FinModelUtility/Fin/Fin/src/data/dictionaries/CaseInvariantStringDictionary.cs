using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace fin.data.dictionaries {
  public class CaseInvariantStringDictionary<T> : IFinDictionary<string, T> {
    private readonly Dictionary<string, T> impl_ = new(StringComparer.OrdinalIgnoreCase);

    public void Clear() => this.impl_.Clear();

    public int Count => this.impl_.Count;
    public IEnumerable<string> Keys => this.impl_.Keys;
    public IEnumerable<T> Values => this.impl_.Values;
    public bool ContainsKey(string key) => this.impl_.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(string key) => this.impl_.Remove(key);

    public T this[string key] {
      get => this.impl_[key];
      set => this.impl_[key] = value;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<(string Key, T Value)> GetEnumerator() {
      foreach (var (key, value) in this.impl_) {
        yield return (key, value);
      }
    }

  }
}
