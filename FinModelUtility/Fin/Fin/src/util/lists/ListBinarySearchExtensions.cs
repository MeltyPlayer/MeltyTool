using System.Collections.Generic;

namespace fin.util.lists;

public interface IStaticAsymmetricComparer<in T, TValue> : IComparer<T> {
  TValue Value { get; }
  int Compare(T lhs, TValue rhs);

  int IComparer<T>.Compare(T lhs, T _) => this.Compare(lhs, this.Value);
}

public static class ListBinarySearchExtensions {
  public static int BinarySearch<T, TValue, TComparer>(
      this List<T> list,
      TComparer comparer)
      where TComparer : IStaticAsymmetricComparer<T, TValue>
    => list.BinarySearch(default!, comparer);
}