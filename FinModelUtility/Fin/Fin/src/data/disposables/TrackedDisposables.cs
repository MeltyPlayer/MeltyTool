using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace fin.data.disposables;

public class TrackedDisposables<T> : IEnumerable<T>
    where T : class, IFinDisposable {
  private readonly LinkedList<WeakReference<T>> impl_ = [];

  public int Count => this.Count();

  public void DisposeAll() {
    foreach (var weakReference in this.impl_) {
      if (weakReference.TryGetTarget(out var value) && !value.IsDisposed) {
        value.Dispose();
      }
    }

    this.impl_.Clear();
  }

  public void Add(T item) => this.impl_.AddLast(new WeakReference<T>(item));

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<T> GetEnumerator() {
    var current = this.impl_.First;
    while (current != null) {
      var weakReference = current.Value;
      if (weakReference.TryGetTarget(out var value) && !value.IsDisposed) {
        yield return value;
      } else {
        this.impl_.Remove(current);
      }

      current = current.Next;
    }
  }
}