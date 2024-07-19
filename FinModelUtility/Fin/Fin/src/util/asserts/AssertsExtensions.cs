namespace fin.util.asserts;

public static class AssertsExtensions {
  public static TExpected AssertAsA<TExpected>(
      this object? instance,
      string? message = null)
    => Asserts.AsA<TExpected>(instance, message);

  public static T AssertNonnull<T>(this T? instance, string? message = null)
    => Asserts.CastNonnull(instance, message);
}