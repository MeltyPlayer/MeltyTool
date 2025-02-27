using System.Collections;
using System.Collections.Generic;

namespace fin.data.lists;

public class ForgivingArrayView<T>(IList<T> impl, T defaultValue)
    : IEnumerable<T> {
  public int Count => impl.Count;

  public T this[int index]
    => index >= 0 && index < impl.Count ? impl[index] : defaultValue;

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<T> GetEnumerator() => impl.GetEnumerator();
}