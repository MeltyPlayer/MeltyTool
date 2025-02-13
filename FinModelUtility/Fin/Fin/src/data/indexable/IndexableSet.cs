using fin.data.sets;

using schema.readOnly;

namespace fin.data.indexable;

[GenerateReadOnly]
public partial interface IIndexableSet<TIndexable> : IFinSet<TIndexable>
    where TIndexable : IIndexable {
  [Const]
  new bool Contains(int index);

  new TIndexable this[int index] { get; }

  [Const]
  new bool TryGetValue(int index, out TIndexable value);
}