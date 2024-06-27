using fin.data.sets;

using schema.readOnly;

namespace fin.data.indexable;

[GenerateReadOnly]
public partial interface IIndexableSet<TIndexable> : IFinSet<TIndexable>
    where TIndexable : IIndexable {
  [Const]
  bool Contains(int index);

  TIndexable this[int index] { get; }

  [Const]
  bool TryGetValue(int index, out TIndexable value);
}