using System;
using System.Collections;
using System.Collections.Generic;

namespace fin.data.queues;

public interface ITuple3Queue<T1, T2, T3> : IEnumerable<(T1, T2, T3)> {
  int Count { get; }

  void Clear();

  void Enqueue((T1, T2, T3) first, params ReadOnlySpan<(T1, T2, T3)> rest);
  void Enqueue(IEnumerable<(T1, T2, T3)> values);

  (T1, T2, T3) Dequeue();
  bool TryDequeue(out T1 value1, out T2 value2, out T3 value3);

  (T1, T2, T3) Peek();
  bool TryPeek(out T1 value1, out T2 value2, out T3 value3);
}

public class FinTuple3Queue<T1, T2, T3> : ITuple3Queue<T1, T2, T3> {
  private readonly FinQueue<(T1, T2, T3)> impl_ = new();

  public FinTuple3Queue() { }

  public FinTuple3Queue((T1, T2, T3) first,
                        params ReadOnlySpan<(T1, T2, T3)> rest)
    => this.Enqueue(first, rest);

  public FinTuple3Queue(IEnumerable<(T1, T2, T3)> values)
    => this.Enqueue(values);

  public int Count => this.impl_.Count;

  public void Clear() => this.impl_.Clear();

  public void Enqueue((T1, T2, T3) first,
                      params ReadOnlySpan<(T1, T2, T3)> rest) {
    this.impl_.Enqueue(first);
    foreach (var value in rest) {
      this.Enqueue(value);
    }
  }

  public void Enqueue(IEnumerable<(T1, T2, T3)> values) {
    foreach (var value in values) {
      this.Enqueue(value);
    }
  }

  public (T1, T2, T3) Dequeue() => this.impl_.Dequeue();

  public bool TryDequeue(out T1 value1, out T2 value2, out T3 value3) {
    if (this.impl_.TryDequeue(out var value)) {
      (value1, value2, value3) = value;
      return true;
    }

    value1 = default!;
    value2 = default!;
    value3 = default!;
    return false;
  }

  public (T1, T2, T3) Peek() => this.impl_.Peek();

  public bool TryPeek(out T1 value1, out T2 value2, out T3 value3) {
    if (this.impl_.TryPeek(out var value)) {
      (value1, value2, value3) = value;
      return true;
    }

    value1 = default!;
    value2 = default!;
    value3 = default!;
    return false;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(T1, T2, T3)> GetEnumerator()
    => this.impl_.GetEnumerator();
}