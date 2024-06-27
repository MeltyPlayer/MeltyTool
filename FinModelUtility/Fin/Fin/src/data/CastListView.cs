using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using fin.util.linq;

namespace fin.data;

/// <summary>
///   A list that stores values in one type but enumerates them as another.
/// </summary>
public class CastListView<TFrom, TTo> : IReadOnlyList<TTo>
    where TFrom : TTo {
  private readonly IReadOnlyList<TFrom> impl_;

  public CastListView(IReadOnlyList<TFrom> impl) {
    this.impl_ = impl;
  }

  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Count;
  }

  public TTo this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_[index];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IEnumerator<TTo> GetEnumerator()
    => this.impl_.CastTo<TFrom, TTo>().GetEnumerator();
}