using System.Collections;
using System.Collections.Generic;

namespace fin.data.queues;

public class DistinctQueue<T> : IFinQueue<T> {
  private readonly HashSet<T> set_ = new();
  private readonly Queue<T> impl_ = new();

  public DistinctQueue() { }

  public DistinctQueue(T first, params T[] rest)
    => this.Enqueue(first, rest);

  public DistinctQueue(IEnumerable<T> values)
    => this.Enqueue(values);

  public int Count => this.impl_.Count;

  public void Clear() {
    this.set_.Clear();
    this.impl_.Clear();
  }

  public void Enqueue(T first, params T[] rest) {
    this.EnqueueImpl_(first);
    foreach (var value in rest) {
      this.EnqueueImpl_(value);
    }
  }

  public void Enqueue(IEnumerable<T> values) {
    foreach (var value in values) {
      this.EnqueueImpl_(value);
    }
  }

  public void EnqueueImpl_(T value) {
    if (this.set_.Add(value)) {
      this.impl_.Enqueue(value);
    }
  }

  public T Dequeue() => this.impl_.Dequeue();
  public bool TryDequeue(out T value) => this.impl_.TryDequeue(out value!);

  public T Peek() => this.impl_.Peek();
  public bool TryPeek(out T value) => this.impl_.TryPeek(out value!);

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<T> GetEnumerator() => this.impl_.GetEnumerator();
}