using System;

namespace fin.util.optional;

public interface IOptional<T> {
  bool Try(out T value);
}

public static class Optional {
  public static FuncOptional<T> Of<T>(Func<T> value) => new(value);
  public static ValueOptional<T> Of<T>(T value) => new(value);
  public static EmptyOptional<T> Empty<T>() => new();
}

public readonly struct EmptyOptional<T> : IOptional<T> {
  public bool Try(out T value) {
    value = default!;
    return false;
  }
}