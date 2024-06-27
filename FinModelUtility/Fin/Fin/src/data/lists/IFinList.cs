using schema.readOnly;

namespace fin.data.lists;

[GenerateReadOnly]
public partial interface IFinList<T> : IFinCollection<T> {
  T this[int index] { get; set; }
}