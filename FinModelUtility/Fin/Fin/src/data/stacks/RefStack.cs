using System;
using System.Runtime.CompilerServices;

namespace fin.data.stacks;

public sealed class RefStack<T> {
  private const int INITIAL_SIZE_ = 50;

  private T[] impl_ = new T[INITIAL_SIZE_];

  public int Count { get; private set; }

  public void Clear() => this.Count = 0;

  public ref T Top {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get { return ref this.impl_[this.Count - 1]; }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Pop() => --this.Count;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Push(in T item) {
    if (this.impl_.Length == this.Count) {
      this.ExtendSize_();
    }

    this.impl_[this.Count++] = item;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void ExtendSize_() {
    var newImpl = new T[this.impl_.Length * 2];
    this.impl_.CopyTo(newImpl);
    this.impl_ = newImpl;
  }
}