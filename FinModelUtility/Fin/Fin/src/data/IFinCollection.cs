using System.Collections.Generic;

using schema.readOnly;

namespace fin.data;

[GenerateReadOnly]
public partial interface IFinCollection<out T> : IEnumerable<T> {
  new int Count { get; }
  void Clear();
}