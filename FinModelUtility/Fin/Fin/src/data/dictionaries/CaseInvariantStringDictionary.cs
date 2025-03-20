using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace fin.data.dictionaries;

public class CaseInvariantStringDictionary<T> : IFinDictionary<string, T> {
  private readonly Dictionary<string, T> impl_
      = new(StringComparer.OrdinalIgnoreCase);

  private readonly Dictionary<string, T>.AlternateLookup<ReadOnlySpan<char>>
      spanImpl_;

  public CaseInvariantStringDictionary() {
    this.impl_ = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
    this.spanImpl_ = this.impl_.GetAlternateLookup<ReadOnlySpan<char>>();
  }

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

  public T this[ReadOnlySpan<char> key] {
    get => this.spanImpl_[key];
    set => this.spanImpl_[key] = value;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(string Key, T Value)> GetEnumerator() {
    foreach (var (key, value) in this.impl_) {
      yield return (key, value);
    }
  }
}