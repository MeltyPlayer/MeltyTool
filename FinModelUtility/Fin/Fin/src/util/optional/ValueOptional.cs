namespace fin.util.optional;

public readonly struct ValueOptional<T>(T inValue) : IOptional<T> {
  public bool Try(out T value) {
    value = inValue;
    return true;
  }
}