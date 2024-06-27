using System.Collections;
using System.Collections.Generic;

namespace fin.data.stacks;

public class FinStack<T> : IFinStack<T> {
  private readonly Stack<T> impl_;

  public FinStack() {
    this.impl_ = new Stack<T>();
  }

  public FinStack(T item) {
    this.impl_ = new Stack<T>();
    this.impl_.Push(item);
  }

  public FinStack(IEnumerable<T> items) {
    this.impl_ = new Stack<T>(items);
  }

  public int Count => this.impl_.Count;

  public T Top {
    get => this.impl_.Peek();
    set {
      this.impl_.Pop();
      this.impl_.Push(value);
    }
  }

  public void Clear() => this.impl_.Clear();

  public bool TryPop(out T item) => this.impl_.TryPop(out item);
  public T Pop() => this.impl_.Pop();
  public void Push(T item) => this.impl_.Push(item);

  public void Push(IEnumerable<T> items) {
    foreach (var item in items) {
      this.Push(item);
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<T> GetEnumerator() => this.impl_.GetEnumerator();
}