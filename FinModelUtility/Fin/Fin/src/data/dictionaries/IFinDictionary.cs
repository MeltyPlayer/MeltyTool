using System.Collections.Generic;

using schema.readOnly;

namespace fin.data.dictionaries;

[GenerateReadOnly]
public partial interface IFinDictionary<TKey, TValue>
    : IFinCollection<(TKey Key, TValue Value)> {
  new IEnumerable<TKey> Keys { get; }
  new IEnumerable<TValue> Values { get; }

  // Have to specify only contains key because "out" method parameters
  // aren't allowed to be covariant:
  // https://github.com/dotnet/csharplang/discussions/5623
  [Const]
  new bool ContainsKey(TKey key);

  new TValue this[TKey key] { get; set; }
  bool Remove(TKey key);
}

public static class FinDictionaryExtensions {
  public static bool TryGetValue<TKey, TValue>(
      this IReadOnlyFinDictionary<TKey, TValue> impl,
      TKey key,
      out TValue value) {
    if (impl.ContainsKey(key)) {
      value = impl[key];
      return true;
    }

    value = default!;
    return false;
  }
}