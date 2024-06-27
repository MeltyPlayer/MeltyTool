using System.Collections;
using System.Collections.Generic;

namespace fin.model.impl;

public interface IVertexAttributeArray<T> : IEnumerable<(int, T)>
    where T : notnull {
  int Count { get; }

  T? Get(int index) => this[index];
  void Set(int index, T? value) => this[index] = value;

  T? this[int index] { get; set; }
}

public class SingleVertexAttribute<T> : IVertexAttributeArray<T>
    where T : notnull {
  private T value_;

  public int Count => 1;

  public T? this[int index] {
    get => index == 0 ? this.value_ : default;
    set {
      //Asserts.Equal(0, index);
      this.value_ = value;
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<(int, T)> GetEnumerator() {
    yield return (0, this.value_);
  }
}

public class SparseVertexAttributeArray<T> : IVertexAttributeArray<T>
    where T : notnull {
  private IList<T?>? impl_;

  public int Count { get; private set; }

  public T? this[int index] {
    get => index < (this.impl_?.Count ?? 0) ? this.impl_[index] : default;
    set {
      this.impl_ ??= new List<T?>();
      if (this.impl_?.Count < index && this.impl_[index] != null) {
        --this.Count;
      }

      while (this.impl_?.Count <= index) {
        this.impl_.Add(default);
      }
      this.impl_![index] = value;

      if (value != null) {
        ++this.Count;
      }
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  public IEnumerator<(int, T)> GetEnumerator() {
    if (this.impl_ != null) {
      for (var i = 0; i < this.impl_.Count; ++i) {
        var value = this.impl_[i];
        if (value != null) {
          yield return (i, value);
        }
      }
    }
  }
}