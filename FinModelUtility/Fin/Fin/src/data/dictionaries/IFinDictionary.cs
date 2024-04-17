using System.Collections.Generic;

using schema.readOnly;

namespace fin.data.dictionaries {
  [GenerateReadOnly]
  public partial interface IFinDictionary<TKey, TValue>
      : IFinCollection<(TKey Key, TValue Value)> {
    IEnumerable<TKey> Keys { get; }
    IEnumerable<TValue> Values { get; }

    [Const]
    bool ContainsKey(TKey key);

    [Const]
    bool TryGetValue(TKey key, out TValue value);

    TValue this[TKey key] { get; set; }
    bool Remove(TKey key);
  }
}