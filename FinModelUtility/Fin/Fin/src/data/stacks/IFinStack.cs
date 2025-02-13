using schema.readOnly;

namespace fin.data.stacks;

/// <summary>
///   Simpler interface for stacks that is easier to implement.
/// </summary>
[GenerateReadOnly]
public partial interface IFinStack<T> : IFinCollection<T> {
  new T Top { get; set; }

  bool TryPop(out T item);
  T Pop();
  void Push(T item);
}