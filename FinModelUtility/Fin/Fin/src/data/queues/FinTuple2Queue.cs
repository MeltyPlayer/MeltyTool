using System.Collections;
using System.Collections.Generic;

namespace fin.data.queues;

public interface ITuple2Queue<T1, T2> : IEnumerable<(T1, T2)> {
  int Count { get; }

  void Clear();

  void Enqueue((T1, T2) first, params (T1, T2)[] rest);
  void Enqueue(IEnumerable<(T1, T2)> values);

  (T1, T2) Dequeue();
  bool TryDequeue(out T1 value1, out T2 value2);

  (T1, T2) Peek();
  bool TryPeek(out T1 value1, out T2 value2);
}

public class FinTuple2Queue<T1, T2> : ITuple2Queue<T1, T2> {
  private readonly FinQueue<(T1, T2)> impl_ = new();

  public FinTuple2Queue() { }

  public FinTuple2Queue((T1, T2) first, params (T1, T2)[] rest)
    => this.Enqueue(first, rest);

  public FinTuple2Queue(IEnumerable<(T1, T2)> values)
    => this.Enqueue(values);

  public int Count => this.impl_.Count;

  public void Clear() => this.impl_.Clear();

  public void Enqueue((T1, T2) first, params (T1, T2)[] rest) {
    this.impl_.Enqueue(first);
    foreach (var value in rest) {
      this.Enqueue(value);
    }
  }

  public void Enqueue(IEnumerable<(T1, T2)> values) {
    foreach (var value in values) {
      this.Enqueue(value);
    }
  }

  public (T1, T2) Dequeue() => this.impl_.Dequeue();

  public bool TryDequeue(out T1 value1, out T2 value2) {
    if (this.impl_.TryDequeue(out var value)) {
      (value1, value2) = value;
      return true;
    }

    value1 = default!;
    value2 = default!;
    return false;
  }

  public (T1, T2) Peek() => this.impl_.Peek();

  public bool TryPeek(out T1 value1, out T2 value2) {
    if (this.impl_.TryPeek(out var value)) {
      (value1, value2) = value;
      return true;
    }

    value1 = default!;
    value2 = default!;
    return false;
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  public IEnumerator<(T1, T2)> GetEnumerator() => this.impl_.GetEnumerator();
}