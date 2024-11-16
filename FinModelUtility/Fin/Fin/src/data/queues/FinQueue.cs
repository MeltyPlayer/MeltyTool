using System;
using System.Collections;
using System.Collections.Generic;

namespace fin.data.queues;

public class FinQueue<T> : IFinQueue<T> {
  private readonly Queue<T> impl_ = new();

  public FinQueue() { }

  public FinQueue(T first, params ReadOnlySpan<T> rest)
    => this.Enqueue(first, rest);

  public FinQueue(IEnumerable<T> values)
    => this.Enqueue(values);

  public int Count => this.impl_.Count;

  public void Clear() => this.impl_.Clear();

  public void Enqueue(T first, params ReadOnlySpan<T> rest) {
    this.impl_.Enqueue(first);
    foreach (var value in rest) {
      this.Enqueue(value);
    }
  }

  public void Enqueue(IEnumerable<T> values) {
    foreach (var value in values) {
      this.Enqueue(value);
    }
  }

  public T Dequeue() => this.impl_.Dequeue();
  public bool TryDequeue(out T value) => this.impl_.TryDequeue(out value!);

  public T Peek() => this.impl_.Peek();
  public bool TryPeek(out T value) => this.impl_.TryPeek(out value!);

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<T> GetEnumerator() => this.impl_.GetEnumerator();
}