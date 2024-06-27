using System.Collections;
using System.Collections.Generic;

using fin.util.asserts;

using schema.readOnly;

namespace fin.data.dictionaries;

[GenerateReadOnly]
public partial interface ISubTypeDictionary<TKey, TValue>
    : IFinCollection<(TKey Key, TValue Value)> {
  [Const]
  TValueSub Get<TValueSub>(TKey key) where TValueSub : TValue;

  void Set<TValueSub>(TKey key, TValueSub value) where TValueSub : TValue;
}

public class SubTypeDictionary<TKey, TValueBase>(
    IFinDictionary<TKey, TValueBase> impl)
    : ISubTypeDictionary<TKey, TValueBase> {
  public SubTypeDictionary() : this(
      new NullFriendlyDictionary<TKey, TValueBase>()) { }

  public void Clear() => impl.Clear();

  public int Count => impl.Count;

  public void Set<TValueSub>(TKey key, TValueSub value)
      where TValueSub : TValueBase
    => impl[key] = value;

  public TValueSub Get<TValueSub>(TKey key) where TValueSub : TValueBase
    => Asserts.AsSubType<TValueBase, TValueSub>(impl[key]);

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(TKey Key, TValueBase Value)> GetEnumerator()
    => impl.GetEnumerator();
}