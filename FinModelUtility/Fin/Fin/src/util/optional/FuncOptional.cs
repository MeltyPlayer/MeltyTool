using System;

namespace fin.util.optional;

public readonly struct FuncOptional<T>(Func<T> handler) : IOptional<T> {
  public bool Try(out T value) {
    value = handler();
    return true;
  }
}