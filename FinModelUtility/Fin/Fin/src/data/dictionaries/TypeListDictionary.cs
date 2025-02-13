using System;
using System.Collections.Generic;
using System.Linq;

using schema.readOnly;

namespace fin.data.dictionaries;

[GenerateReadOnly]
public partial interface ITypeListDictionary<TValue> {
  [Const]
  new bool HasList<TKey>() where TKey : TValue;

  [Const]
  new IEnumerable<TKey> GetValues<TKey>() where TKey : TValue;

  void Add(Type key, TValue value);
}

public static class TypeListDictionaryExtensions {
  public static bool TryGetValues<TKey, TValue>(
      this ITypeListDictionary<TValue> impl,
      TKey key,
      out IEnumerable<TKey> list) where TKey : TValue {
    if (impl.HasList<TKey>()) {
      list = impl.GetValues<TKey>();
      return true;
    }

    list = default;
    return false;
  }

  public static bool TryGetValues<TKey, TValue>(
      this IReadOnlyTypeListDictionary<TValue> impl,
      TKey key,
      out IEnumerable<TKey> list) where TKey : TValue {
    if (impl.HasList<TKey>()) {
      list = impl.GetValues<TKey>();
      return true;
    }

    list = default;
    return false;
  }
}

public class TypeListDictionary<TValue>(IListDictionary<Type, TValue> impl)
    : ITypeListDictionary<TValue> {
  public TypeListDictionary() : this(new ListDictionary<Type, TValue>()) { }

  public bool HasList<TKey>() where TKey : TValue => impl.HasList(typeof(TKey));

  public IEnumerable<TKey> GetValues<TKey>() where TKey : TValue
    => impl[typeof(TKey)].Cast<TKey>();

  public void Add(Type key, TValue value) => impl.Add(key, value);
}