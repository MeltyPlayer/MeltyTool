using System.Collections;
using System.Collections.Generic;

namespace fin.data.sets;

public class FinSet<T>(ISet<T> impl) : IFinSet<T> {
  public FinSet() : this(new HashSet<T>()) { }

  public int Count => impl.Count;
  public bool Contains(T value) => impl.Contains(value);
  public void Clear() => impl.Clear();
  public bool Add(T value) => impl.Add(value);
  public bool Remove(T value) => impl.Remove(value);
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<T> GetEnumerator() => impl.GetEnumerator();
}