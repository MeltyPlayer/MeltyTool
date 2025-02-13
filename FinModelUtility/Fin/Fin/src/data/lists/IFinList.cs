using schema.readOnly;

namespace fin.data.lists;

[GenerateReadOnly]
public partial interface IFinList<T> : IFinCollection<T> {
  new T this[int index] { get; set; }
}