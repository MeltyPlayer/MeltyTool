using System.Collections.Generic;

namespace fin.data.indexable;

public class IndexableComparer : IComparer<IIndexable> {
  public static IndexableComparer Instance { get; } = new();

  public int Compare(IIndexable? x, IIndexable? y) {
    if (ReferenceEquals(x, y)) {
      return 0;
    }

    if (ReferenceEquals(null, y)) {
      return 1;
    }

    if (ReferenceEquals(null, x)) {
      return -1;
    }

    return x.Index.CompareTo(y.Index);
  }
}