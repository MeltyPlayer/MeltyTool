using schema.readOnly;

namespace fin.data.sets;

[GenerateReadOnly]
public partial interface ICovariantFinSet<T> : IFinCollection<T> {
  bool Add(T value);
  bool Remove(T value);
}

/// <summary>
///   Simpler interface for sets that is easier to implement.
/// </summary>
[GenerateReadOnly]
public partial interface IFinSet<T> : ICovariantFinSet<T> {
  [Const]
  new bool Contains(T value);
}