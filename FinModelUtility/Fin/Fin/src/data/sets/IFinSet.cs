using schema.readOnly;

namespace fin.data.sets {
  /// <summary>
  ///   Simpler interface for sets that is easier to implement.
  /// </summary>
  [GenerateReadOnly]
  public partial interface IFinSet<T> : IFinCollection<T> {
    bool Add(T value);
    bool Remove(T value);

    [Const]
    bool Contains(T value);
  }
}