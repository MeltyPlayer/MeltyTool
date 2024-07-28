using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace fin.data.sets;

public class CastSet<TIn, TOut>(IFinSet<TIn> impl) : IFinSet<TOut>
    where TIn : TOut {
  public CastSet(ISet<TIn> impl) : this(new FinSet<TIn>(impl)) { }
  public CastSet() : this(new FinSet<TIn>()) { }

  public int Count => impl.Count;
  public void Clear() => impl.Clear();

  public bool Add(TOut value) => impl.Add(((TIn) value)!);
  public bool Remove(TOut value) => impl.Remove(((TIn) value)!);
  public bool Contains(TOut value) => impl.Contains(((TIn) value)!);

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<TOut> GetEnumerator() => impl.Cast<TOut>().GetEnumerator();
}