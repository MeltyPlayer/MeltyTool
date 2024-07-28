namespace fin.data.dictionaries;

public class BidirectionalDictionary<T1, T2> {
  private readonly NullFriendlyDictionary<T1, T2> impl1To2_ = new();
  private readonly NullFriendlyDictionary<T2, T1> impl2To1_ = new();

  public int Count => this.impl1To2_.Count;

  public void Clear() {
    this.impl1To2_.Clear();
    this.impl2To1_.Clear();
  }

  public bool ContainsKey(T1 key) => this.impl1To2_.ContainsKey(key);
  public bool ContainsKey(T2 key) => this.impl2To1_.ContainsKey(key);

  public T2 this[T1 key] {
    get => this.impl1To2_[key];
    set {
      this.impl1To2_[key] = value;
      this.impl2To1_[value] = key;
    }
  }

  public T1 this[T2 key] {
    get => this.impl2To1_[key];
    set {
      this.impl2To1_[key] = value;
      this.impl1To2_[value] = key;
    }
  }

  public bool Remove(T1 key) {
    if (this.impl1To2_.Remove(key, out var value)) {
      this.impl2To1_.Remove(value);
      return true;
    }

    return false;
  }

  public bool Remove(T2 key) {
    if (this.impl2To1_.Remove(key, out var value)) {
      this.impl1To2_.Remove(value);
      return true;
    }

    return false;
  }
}