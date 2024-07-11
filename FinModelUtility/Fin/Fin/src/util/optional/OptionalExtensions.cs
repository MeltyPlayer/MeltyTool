namespace fin.util.optional;

public static class OptionalExtensions {
  public static T? GetOrNull<T>(this IOptional<T>? optional) {
    if (optional?.Try(out var value) ?? false) {
      return value;
    }

    return default;
  }
}