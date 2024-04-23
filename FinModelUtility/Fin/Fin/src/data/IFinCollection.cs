using System.Collections.Generic;

using schema.readOnly;

namespace fin.data {
  [GenerateReadOnly]
  public partial interface IFinCollection<out T> : IEnumerable<T> {
    int Count { get; }
    void Clear();
  }
}