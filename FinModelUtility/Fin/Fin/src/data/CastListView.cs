using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using fin.util.linq;

namespace fin.data;

/// <summary>
///   A list that stores values in one type but enumerates them as another.
/// </summary>
public class CastListView<TFrom, TTo>(IReadOnlyList<TFrom> impl)
    : IReadOnlyList<TTo>
    where TFrom : TTo {
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => impl.Count;
  }

  public TTo this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => impl[index];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IEnumerator<TTo> GetEnumerator()
    => impl.CastTo<TFrom, TTo>().GetEnumerator();
}