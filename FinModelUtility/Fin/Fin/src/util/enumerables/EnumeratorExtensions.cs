using System.Collections.Generic;

namespace fin.util.enumerables;

public static class EnumeratorExtensions {
  public static IEnumerator<T> ToEnumerator<T>(this IEnumerable<T> en)
    => en.GetEnumerator();

  public static bool TryMoveNext<T>(this IEnumerator<T> values, out T value) {
    if (!values.MoveNext()) {
      value = default;
      return false;
    }

    value = values.Current;
    return true;
  }
}